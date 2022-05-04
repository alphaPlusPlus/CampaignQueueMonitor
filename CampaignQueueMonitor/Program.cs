using System;
using CampaignQueueMonitor.Clients;
using CampaignQueueMonitor.Clients.Interfaces;
using Coravel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Web;

namespace CampaignQueueMonitor
{
    public class Program
    {
        private static string environment;
        private static IConfigurationRoot configuration;

        public static void Main(string[] args)
        {
            var basePath = AppContext.BaseDirectory;

            configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            environment = configuration["DOTNET_ENVIRONMENT"];

            configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile($"appsettings.{environment}.json")
                .Build();

            var loggingConfiguration = CreateLoggingConfiguration();

            var logger = NLogBuilder
                .ConfigureNLog(loggingConfiguration)
                .GetCurrentClassLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();

                host.Services.UseScheduler(x => x
                        .Schedule<CampaignQueueMonitorJob>()
                        .EveryTenSeconds()
                        .Zoned(TimeZoneInfo.Local)
                        .PreventOverlapping(nameof(CampaignQueueMonitorJob))
                    )
                    .OnError(e => logger.Error(e));


                host.Run();
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddScheduler();

                    services.AddTransient<CampaignQueueMonitorJob>();

                    services.AddTransient<ICampaignQueueMonitor, CampaignQueueMonitor>();

                    services.AddHttpClient<IZendeskClient, ZendeskCountClient>(client =>
                    {
                        client.BaseAddress = new Uri(configuration["ZendeskBaseUrl"]);
                        client.DefaultRequestHeaders.Add("Authorization" , configuration["ZendeskAuthToken"]);
                    });

                    services.AddHttpClient<IServerCountClient, ServerCountClient>();

                    services.AddHttpClient<IVisualiserSeriesClient, VisualiserSeriesClient>(client =>
                    {
                        client.BaseAddress = new Uri($"{configuration["VisualiserSeriesUrl"]}?api_key={configuration["VisualiserSeriesApiKey"]}");
                    });
                })
                .ConfigureWebHostDefaults(options =>
                {
                    options.UseStartup<Startup>();
                })
                .UseWindowsService()
                .UseNLog();

        private static LoggingConfiguration CreateLoggingConfiguration()
        {
            var loggingConfiguration = new LoggingConfiguration();

            var consoleTarget = new ConsoleTarget("Console");

            loggingConfiguration.AddTarget(consoleTarget);

            loggingConfiguration.AddRule(
                LogLevel.Info,
                LogLevel.Fatal,
                consoleTarget);

            return loggingConfiguration;
        }
    }
}
