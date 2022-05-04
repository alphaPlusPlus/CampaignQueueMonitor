using System.Collections.Generic;

namespace CampaignQueueMonitor.Domain
{
    public class SeriesItem
    {
        public string Metric { get; set; }

        public List<List<long>> Points { get; set; }

        public string Type { get; set; }
    }
}