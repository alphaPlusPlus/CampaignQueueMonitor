using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CampaignQueueMonitor.Clients.Interfaces;

namespace CampaignQueueMonitor.Clients
{
    public class ServerCountClient : IServerCountClient, IDisposable
    {
        private const string NewCountRegex = "(?<=new count: \\()(.*)(?=\\))";
        private readonly HttpClient httpClient;
        

        public ServerCountClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<int?> GetCountAsync(int serverId)
        {
            var response = await httpClient.GetAsync($"http://{serverId}.localhost.com/count");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var match = new Regex(NewCountRegex, RegexOptions.IgnoreCase).Match(html);

            if (match.Success)
            {
                return int.Parse(match.Value);
            }

            return null;
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}