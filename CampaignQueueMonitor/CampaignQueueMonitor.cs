using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CampaignQueueMonitor.Domain;
using CampaignQueueMonitor.Clients.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CampaignQueueMonitor
{
    public class CampaignQueueMonitor : ICampaignQueueMonitor
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<CampaignQueueMonitor> logger;
        private readonly IZendeskClient zendeskClient;
        private readonly IServerCountClient serverCountClient;
        private readonly IVisualiserSeriesClient visualiserSeriesClient;

        public CampaignQueueMonitor(
            IConfiguration configuration,
            ILogger<CampaignQueueMonitor> logger,
            IZendeskClient zendeskClient,
            IServerCountClient serverCountClient,
            IVisualiserSeriesClient visualiserSeriesClient)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.zendeskClient = zendeskClient;
            this.serverCountClient = serverCountClient;
            this.visualiserSeriesClient = visualiserSeriesClient;
        }

        public async Task Run()
        {
            logger.LogInformation("CampaignQueueMonitor:Run");

            // here i don't really understand that part, will this server list come from DB?
            int[] serverIds = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

            // i did this part in parallel assuming that the order of these requests dos not matter 
            await Task.WhenAll(serverIds.Select(serverId =>
            {
                return serverCountClient.GetCountAsync(serverId).ContinueWith(count =>
                {
                    if (!count.Result.HasValue) 
                        return;

                    logger.LogInformation($"Server: {serverId} Campaign Queue Size: {count.Result.Value}");

                    long epochTimestamp = SystemTime.DateTimeOffset.ToUnixTimeSeconds();

                    var items = new List<SeriesItem>
                    {
                        new SeriesItem
                        {
                            Metric = $"Campaign.{serverId}",
                            Type = "Count",
                            Points = new List<List<long>>
                            {
                                new List<long>
                                {
                                    epochTimestamp,
                                    count.Result.Value
                                }
                            }
                        }
                    };

                    visualiserSeriesClient.SendAsync(items);
                });
            }));

            await zendeskClient.GetCountAsync().ContinueWith(count =>
            {
                if (!count.Result.HasValue) 
                    return;

                logger.LogInformation($"Zendesk Engineering Ticket count: {count.Result.Value}");

                Thread.Sleep(50);
                var seriesItems = new List<SeriesItem>
                {
                    new SeriesItem
                    {
                        Metric = "Zendesk.Metric",
                        Type = "Count",
                        Points = new List<List<long>>
                        {
                            new List<long>
                            {
                                SystemTime.DateTimeOffset.ToUnixTimeSeconds(),
                                count.Result.Value
                            }
                        }
                    }
                };

                visualiserSeriesClient.SendAsync(seriesItems);
            });
        }

        public void Dispose()
        {
            logger.LogInformation("CampaignQueueMonitor:Dispose");
        }
    }
}