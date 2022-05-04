using System.Collections.Generic;
using System.Threading.Tasks;
using CampaignQueueMonitor.Domain;

namespace CampaignQueueMonitor.Clients.Interfaces
{
    public interface IVisualiserSeriesClient
    {
        Task<bool> SendAsync(List<SeriesItem> items);
    }
}