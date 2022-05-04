using System;
using System.Threading.Tasks;
using Coravel.Invocable;

namespace CampaignQueueMonitor
{
    public class CampaignQueueMonitorJob : IInvocable, IDisposable
    {
        private readonly ICampaignQueueMonitor campaignQueueMonitor;


        public CampaignQueueMonitorJob(ICampaignQueueMonitor campaignQueueMonitor)
        {
            this.campaignQueueMonitor = campaignQueueMonitor;
        }

        public async Task Invoke()
        {
            await campaignQueueMonitor.Run();
        }

        public void Dispose()
        {
            campaignQueueMonitor.Dispose();
        }
    }
}