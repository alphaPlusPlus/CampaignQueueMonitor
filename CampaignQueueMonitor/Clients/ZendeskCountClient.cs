using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CampaignQueueMonitor.Clients.Interfaces;
using CampaignQueueMonitor.Domain;

namespace CampaignQueueMonitor.Clients
{
    public class ZendeskCountClient : IZendeskClient, IDisposable
    {
        private readonly HttpClient httpClient;

        public ZendeskCountClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<int?> GetCountAsync()
        {
            var response = await httpClient.GetAsync("/count");
            response.EnsureSuccessStatusCode();

            return response.Content.ReadFromJsonAsync<ZendeskCountResponse>().Result?.Count;
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}