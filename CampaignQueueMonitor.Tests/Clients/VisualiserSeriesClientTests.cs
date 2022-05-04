using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using CampaignQueueMonitor.Clients;
using CampaignQueueMonitor.Clients.Interfaces;
using CampaignQueueMonitor.Domain;
using CampaignQueueMonitor.Tests.Helpers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CampaignQueueMonitor.Tests.Clients
{
    public class VisualiserSeriesClientTests
    {
        private readonly IFixture _fixture;
        private readonly IVisualiserSeriesClient _visualiserSeriesClient;
        private readonly HttpClient _mockHttpClient;
        private readonly string _url;
        private readonly List<SeriesItem> _seriesItems;

        public VisualiserSeriesClientTests()
        {
            _fixture = new Fixture();
            _url = "https://localhost/api/v1/series/";
            _seriesItems = _fixture.CreateMany<SeriesItem>().ToList();

            HttpResponseMessage httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            };

            _mockHttpClient = new HttpClient(new FakeHttpMessageHandler(
                async (request, cancellationToken) =>
                {
                    var responseMessage = httpResponseMessage;

                    return await Task.FromResult(responseMessage);
                }))
            {
                BaseAddress = new Uri(_url)

            };


            _visualiserSeriesClient = new VisualiserSeriesClient(_mockHttpClient);

        }

        [Fact]
        public void OkStatusCodeIsReturned()
        {
            _visualiserSeriesClient.SendAsync(_seriesItems).Result.Should().Be(true);
        }
    }
}