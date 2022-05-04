using System;
using FluentAssertions;
using FluentAssertions.Equivalency;

namespace CampaignQueueMonitor.Tests.Helpers
{
    public static class EquivalencyExtensions
    {
        public static bool IsEquivalentTo<T>(this T source, T expectation)
        {
            try
            {
                source.Should().BeEquivalentTo(expectation);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool IsEquivalentTo<T>(this T source, T expectation,
            Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> config)
        {
            try
            {
                source.Should().BeEquivalentTo(expectation, config);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}