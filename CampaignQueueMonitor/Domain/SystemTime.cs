using System;

namespace CampaignQueueMonitor.Domain
{
    public class SystemTime
    {
        private static DateTime? frozenNow;

        public static DateTime Now => frozenNow ?? DateTime.Now;

        public static DateTimeOffset DateTimeOffset => frozenNow ?? DateTime.Now;

        public static bool IsFrozen => frozenNow.HasValue;

        public static void Freeze() => frozenNow = DateTime.Now;

        public static void Freeze(DateTime now) => frozenNow = now;
    }
}