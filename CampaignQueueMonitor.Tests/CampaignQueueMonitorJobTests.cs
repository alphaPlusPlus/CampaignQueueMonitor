using NSubstitute;
using Xunit;

namespace CampaignQueueMonitor.Tests
{
    public class CampaignQueueMonitorJobTests
    {
        private readonly ICampaignQueueMonitor campaignQueueMonitor;
        private readonly CampaignQueueMonitorJob campaignQueueMonitorJob;

        public CampaignQueueMonitorJobTests()
        {
            campaignQueueMonitor = Substitute.For<ICampaignQueueMonitor>();

            campaignQueueMonitorJob = new CampaignQueueMonitorJob(campaignQueueMonitor);
        }

        public class InvokeTests : CampaignQueueMonitorJobTests
        {
            [Fact]
            public void RunIsCalled()
            {
                campaignQueueMonitorJob.Invoke().Wait();

                campaignQueueMonitor.Received(1).Run();
            }
        }
    }
}