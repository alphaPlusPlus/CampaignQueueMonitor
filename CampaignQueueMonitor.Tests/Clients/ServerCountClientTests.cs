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
    public class ServerCountClientTests
    {
        private readonly IFixture _fixture;
        private readonly string _html;
        private readonly IServerCountClient _serverCountClient;
        private FakeHttpMessageHandler _fakeHttpMessageHandler;
        private readonly HttpClient _mockHttpClient;
        private readonly int _count;
        private readonly int _serverId;
        private readonly string _url;

        public ServerCountClientTests()
        {
            _fixture = new Fixture();
            _count = _fixture.Create<int>();
            _serverId = _fixture.Create<int>();
            _url = $"http://{_serverId}.localhost.com/count";

            _html = @"<!DOCTYPE html>
                    <html>
                    <body>

                    <h1>new count: (${count}$)</h1>

                    </body>
                    </html>";

            _html = _html.Replace("${count}$", _count.ToString());

            HttpResponseMessage httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(_html)
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



            _serverCountClient = new ServerCountClient(_mockHttpClient);

        }

        [Fact]
        public void CountIsReturned()
        {
            _serverCountClient.GetCountAsync(_serverId).Result.Should().Be(_count);
        }
    }
}