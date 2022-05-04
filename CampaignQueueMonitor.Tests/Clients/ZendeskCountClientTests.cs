using System;
using System.Collections.Generic;
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
    public class ZendeskCountClientTests
    {
        private readonly IFixture _fixture;
        private readonly string _json;
        private readonly IZendeskClient _zendeskCountClient;
        private readonly HttpClient _mockHttpClient;
        private readonly int _count;
        private readonly string _url;

        public ZendeskCountClientTests()
        {
            _fixture = new Fixture();
            _count = _fixture.Create<int>();
            _url = $"http://localhost.com/count";

            _json = "{ \"count\" : " + _count + " }";

            HttpResponseMessage httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(_json)
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

            _zendeskCountClient = new ZendeskCountClient(_mockHttpClient);

        }

        [Fact]
        public void CountIsReturned()
        {
            _zendeskCountClient.GetCountAsync().Result.Should().Be(_count);
        }
    }
}