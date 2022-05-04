using System;
using System.Threading.Tasks;

namespace CampaignQueueMonitor
{
    public interface ICampaignQueueMonitor : IDisposable
    {
        Task Run();
    }
}