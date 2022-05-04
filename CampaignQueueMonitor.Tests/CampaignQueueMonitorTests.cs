using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using CampaignQueueMonitor.Domain;
using CampaignQueueMonitor.Clients.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using CampaignQueueMonitor.Tests.Helpers;

namespace CampaignQueueMonitor.Tests
{
    public class CampaignQueueMonitorTests
    {
        private readonly long epochTimestamp;
        private readonly IFixture fixture;
        private readonly ILogger<CampaignQueueMonitor> logger;
        private readonly IServerCountClient serverCountClient;
        private readonly ICampaignQueueMonitor campaignQueueMonitor;
        private readonly int? zendeskCount;
        private readonly IVisualiserSeriesClient visualiserSeriesClient;
        private readonly int[] serverIds;
        private readonly Dictionary<int, int?> serverCounts;


        public CampaignQueueMonitorTests()
        {
            SystemTime.Freeze(new DateTime(2020, 1, 1, 10, 10, 00));
            epochTimestamp = SystemTime.DateTimeOffset.ToUnixTimeSeconds();
            serverIds = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};
            serverCounts = new Dictionary<int, int?>();

            fixture = new Fixture();

            var configuration = Substitute.For<IConfiguration>();
            logger = Substitute.For<ILogger<CampaignQueueMonitor>>();

            var zendeskClient = Substitute.For<IZendeskClient>();
            zendeskCount = fixture.Create<int?>();
            zendeskClient.GetCountAsync().ReturnsForAnyArgs(Task.FromResult(zendeskCount));

            serverCountClient = Substitute.For<IServerCountClient>();

            visualiserSeriesClient = Substitute.For<IVisualiserSeriesClient>();
            var visualiserSeriesClientResponseMessage = Substitute.For<HttpResponseMessage>();
            visualiserSeriesClient.SendAsync(default).ReturnsForAnyArgs(Task.FromResult(visualiserSeriesClientResponseMessage.IsSuccessStatusCode));

            campaignQueueMonitor = new CampaignQueueMonitor(configuration, logger, zendeskClient, serverCountClient, visualiserSeriesClient);
        }

        public class RunTests : CampaignQueueMonitorTests
        {
            public class ZendeskMessageIsSent : RunTests
            {
                private readonly List<SeriesItem> message;
                public ZendeskMessageIsSent()
                {
                    if (zendeskCount.HasValue)
                    {
                        message = new List<SeriesItem>
                        {
                            new SeriesItem()
                            {
                                Metric = "Zendesk.Metric",
                                Type = "Count",
                                Points = new List<List<long>>()
                                {
                                    new List<long>()
                                    {
                                        epochTimestamp,
                                        zendeskCount.Value
                                    }
                                }
                            }
                        };
                    }
                }

                [Fact]
                public void MessageIsSent()
                {
                    campaignQueueMonitor.Run().Wait();

                    visualiserSeriesClient.Received(1)
                        .SendAsync(Arg.Is<List<SeriesItem>>(x => x.IsEquivalentTo(message)));
                }

                [Fact]
                public void InfoIsLogged()
                {
                    campaignQueueMonitor.Run().Wait();

                    logger.Received(1).LogInformation($"Zendesk Engineering Ticket count: {zendeskCount}");
                }
            }

            public class ServerMessagesAreSent : RunTests
            {
                public class CountsAreNotNull : ServerMessagesAreSent
                {
                    private readonly Dictionary<int, List<SeriesItem>> messages;
                    public CountsAreNotNull()
                    {
                        messages = new Dictionary<int, List<SeriesItem>>();
                        foreach (var serverId in serverIds)
                        {
                            int? count = fixture.Create<int>();
                            serverCounts.Add(serverId, count);
                            serverCountClient.GetCountAsync(Arg.Is(serverId)).Returns(Task.FromResult(count));

                        }

                        foreach (var serverId in serverIds)
                        {
                            if (!serverCounts[serverId].HasValue)
                            {
                                continue;
                            }

                            messages.Add(serverId, new List<SeriesItem>
                            {
                                new SeriesItem
                                {
                                    Metric = $"Campaign.{serverId}",
                                    Type = "Count",
                                    Points = new List<List<long>>
                                    {
                                        new List<long>
                                        {
                                            SystemTime.DateTimeOffset.ToUnixTimeSeconds(),
                                            serverCounts[serverId].Value
                                        }
                                    }
                                }
                            });
                        }
                    }

                    [Fact]
                    public void MessagesAreSent()
                    {
                        campaignQueueMonitor.Run().Wait();

                        foreach (var serverId in serverIds)
                        {
                            visualiserSeriesClient.Received(1)
                                .SendAsync(Arg.Is<List<SeriesItem>>(x => x.IsEquivalentTo(messages[serverId])));
                        }
                    }

                    [Fact]
                    public void InfosAreLogged()
                    {
                        campaignQueueMonitor.Run().Wait();

                        foreach (var serverId in serverIds)
                        {
                            logger.Received(1).LogInformation($"Server: {serverId} Campaign Queue Size: {serverCounts[serverId]}");
                        }
                    }
                }


                public class SomeCountsAreNull : ServerMessagesAreSent
                {
                    private readonly Dictionary<int, List<SeriesItem>> messages;
                    public SomeCountsAreNull()
                    {
                        messages = new Dictionary<int, List<SeriesItem>>();
                        foreach (var serverId in serverIds)
                        {
                            int? count = fixture.Create<int>();

                            if (serverId % 2 == 0)
                            {
                                count = null;
                            }

                            serverCounts.Add(serverId, count);
                            serverCountClient.GetCountAsync(Arg.Is(serverId)).Returns(Task.FromResult(count));

                        }

                        foreach (var serverId in serverIds)
                        {
                            if (!serverCounts[serverId].HasValue)
                            {
                                continue;
                            }

                            messages.Add(serverId, new List<SeriesItem>
                            {
                                new SeriesItem
                                {
                                    Metric = $"Campaign.{serverId}",
                                    Type = "Count",
                                    Points = new List<List<long>>
                                    {
                                        new List<long>
                                        {
                                            SystemTime.DateTimeOffset.ToUnixTimeSeconds(),
                                            serverCounts[serverId].Value
                                        }
                                    }
                                }
                            });
                        }
                    }

                    [Fact]
                    public void MessagesAreSent()
                    {
                        campaignQueueMonitor.Run().Wait();

                        foreach (var serverCount in serverCounts)
                        {
                            if (!serverCount.Value.HasValue)
                            {
                                continue;
                            }

                            visualiserSeriesClient.Received(1)
                                .SendAsync(Arg.Is<List<SeriesItem>>(x => x.IsEquivalentTo(messages[serverCount.Key])));
                        }
                    }

                    [Fact]
                    public void InfosAreLogged()
                    {
                        campaignQueueMonitor.Run().Wait();

                        foreach (var serverCount in serverCounts)
                        {
                            if (!serverCount.Value.HasValue)
                            {
                                continue;
                            }

                            logger.Received(1).LogInformation($"Server: {serverCount.Key} Campaign Queue Size: {serverCount.Value}");
                        }
                    }
                }
            }
        }
    }
}