using System.Threading.Tasks;

namespace CampaignQueueMonitor.Clients.Interfaces
{
    public interface IZendeskClient
    {
        Task<int?> GetCountAsync();
    }
}