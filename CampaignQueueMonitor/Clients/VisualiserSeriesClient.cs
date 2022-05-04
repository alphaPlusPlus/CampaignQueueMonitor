using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CampaignQueueMonitor.Clients.Interfaces;
using CampaignQueueMonitor.Domain;

namespace CampaignQueueMonitor.Clients
{
    public class VisualiserSeriesClient : IVisualiserSeriesClient, IDisposable
    {
        private readonly HttpClient httpClient;

        public VisualiserSeriesClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<bool> SendAsync(List<SeriesItem> items)
        {
            var request = new VisualiserSeriesRequest
            {
                Series = items
            };

            var response = await httpClient.PostAsJsonAsync("/postdata", request);

            return response.IsSuccessStatusCode;
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}