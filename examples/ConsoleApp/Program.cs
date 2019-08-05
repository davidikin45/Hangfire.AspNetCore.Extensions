using Hangfire;
using Hangfire.Annotations;
using Hangfire.AspNetCore.Extensions;
using Hangfire.Common;
using Hangfire.Initialization;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        //https://github.com/HangfireIO/Hangfire/blob/a07ad0b9926923db75747d92796c5a9db39c1a87/samples/NetCoreSample/Program.cs
        static async Task Main(string[] args)
        {
            //vconstar string connectionString = "Server=(localdb)\\mssqllocaldb;Database=Hangfire;Trusted_Connection=True;MultipleActiveResultSets=true;";
            //const string connectionString = "Data Source=:hangfire.db;";
            const string connectionString = "";

            var hostBuilder = new HostBuilder()
                .ConfigureLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Information))
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHangfireServer("console", connectionString, null, options => {
                        var jobFilters = new JobFilterCollection();
                        jobFilters.Add(new CaptureCultureAttribute());
                        jobFilters.Add(new AutomaticRetryAttribute());
                        jobFilters.Add(new StatisticsHistoryAttribute());
                        jobFilters.Add(new ContinuationsSupportAttribute());

                        jobFilters.Add(new HangfireLoggerAttribute());
                        jobFilters.Add(new HangfirePreserveOriginalQueueAttribute());

                        options.FilterProvider = new JobFilterProviderCollection(jobFilters, new JobFilterAttributeFilterProvider());
                    });

                    services.AddHostedService<RecurringJobsService>();
                });

            await hostBuilder.RunConsoleAsync();
        }

        internal class RecurringJobsService : BackgroundService
        {
            private readonly IBackgroundJobClient _backgroundJobs;
            private readonly IRecurringJobManager _recurringJobs;
            private readonly ILogger<RecurringJobScheduler> _logger;

            public RecurringJobsService(
                [NotNull] IBackgroundJobClient backgroundJobs,
                [NotNull] IRecurringJobManager recurringJobs,
                [NotNull] ILogger<RecurringJobScheduler> logger)
            {
                _backgroundJobs = backgroundJobs ?? throw new ArgumentNullException(nameof(backgroundJobs));
                _recurringJobs = recurringJobs ?? throw new ArgumentNullException(nameof(recurringJobs));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            protected override Task ExecuteAsync(CancellationToken stoppingToken)
            {
                try
                {
                    _recurringJobs.AddOrUpdate("seconds", () => Console.WriteLine("Hello, seconds!"), "*/15 * * * * *");
                    _recurringJobs.AddOrUpdate("minutely", () => Console.WriteLine("Hello, world!"), Cron.Minutely);
                    _recurringJobs.AddOrUpdate("hourly", () => Console.WriteLine("Hello"), "25 15 * * *");
                    _recurringJobs.AddOrUpdate("neverfires", () => Console.WriteLine("Can only be triggered"), "0 0 31 2 *");

                    _recurringJobs.AddOrUpdate("Hawaiian", () => Console.WriteLine("Hawaiian"), "15 08 * * *", TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"));
                    _recurringJobs.AddOrUpdate("UTC", () => Console.WriteLine("UTC"), "15 18 * * *");
                    _recurringJobs.AddOrUpdate("Russian", () => Console.WriteLine("Russian"), "15 21 * * *", TimeZoneInfo.Local);
                }
                catch (Exception e)
                {
                    _logger.LogError("An exception occurred while creating recurring jobs.", e);
                }

                return Task.CompletedTask;
            }
        }
    }
}
