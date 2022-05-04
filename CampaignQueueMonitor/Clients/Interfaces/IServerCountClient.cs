using System.Threading.Tasks;

namespace CampaignQueueMonitor.Clients.Interfaces
{
    public interface IServerCountClient
    {
        Task<int?> GetCountAsync(int serverId);
    }
}