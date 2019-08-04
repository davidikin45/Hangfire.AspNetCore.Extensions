using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Initialization;
using Hangfire.Server;
using Hangfire.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.AspNetCore.Extensions
{
    public static class HangfireServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the hangfire services to the application.
        /// </summary>
        public static IServiceCollection AddHangfire(this IServiceCollection services, string serverName, string connectionString = "", Action<JobStorageOptions> configJobStorage = null, Action<BackgroundJobServerOptions> configAction = null, IEnumerable<IBackgroundProcess> additionalProcesses = null)
        {
            services.AddHangfire(config =>
            {
                config
                 .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                 .UseSimpleAssemblyNameTypeSerializer()
                 .UseRecommendedSerializerSettings();

                config.UseFilter(new HangfireLoggerAttribute());
                config.UseFilter(new HangfirePreserveOriginalQueueAttribute());

                if (connectionString != null)
                {
                    //Initializes Hangfire Schema if PrepareSchemaIfNecessary = true
                    var storage = HangfireJobStorage.GetJobStorage(connectionString, configJobStorage).JobStorage;

                    config.UseStorage(storage);
                    //config.UseMemoryStorage();
                    //config.UseSqlServerStorage(connectionString);
                    //config.UseSQLiteStorage(connectionString);
                }
            });

            if (connectionString != null)
            {
                //Launches Server as IHostedService
                services.AddHangfireServer(serverName, configAction, additionalProcesses);
            }

            return services;
        }

        /// <summary>
        /// Adds the hangfire services to the application and starts hangfire server in memory.
        /// </summary>
        public static IServiceCollection AddHangfireInMemory(this IServiceCollection services, string serverName, Action<JobStorageOptions> configJobStorage = null, Action<BackgroundJobServerOptions> configAction = null, IEnumerable<IBackgroundProcess> additionalProcesses = null)
        {
            return services.AddHangfire(serverName, "", configJobStorage, configAction, additionalProcesses);
        }

        /// <summary>
        /// Adds the hangfire services to the application and starts hangfire server in memory.
        /// </summary>
        public static IServiceCollection AddHangfireSQLiteInMemory(this IServiceCollection services, string serverName, Action<JobStorageOptions> configJobStorage = null, Action<BackgroundJobServerOptions> configAction = null, IEnumerable<IBackgroundProcess> additionalProcesses = null)
        {
            return services.AddHangfire(serverName, "DataSource=:memory:;", configJobStorage, configAction, additionalProcesses);
        }

        //IBackgroundJobClient and IRecurringJobManager will only work when storage setup via services.AddHangfire
        public static IServiceCollection AddHangfireServer(this IServiceCollection services, string serverName, Action<BackgroundJobServerOptions> configAction = null, IEnumerable<IBackgroundProcess> additionalProcesses = null, JobStorage storage = null)
        {
            return services.AddTransient<IHostedService, BackgroundJobServerHostedService>(provider =>
            {
                ThrowIfNotConfigured(provider);

                var options = new BackgroundJobServerOptions
                {
                    ServerName = serverName,
                    //Queues = new[] { serverName, EnqueuedState.DefaultQueue }
                };

                if (!string.IsNullOrEmpty(options.ServerName) && !options.Queues.Contains(options.ServerName))
                {
                    var queues = options.Queues.ToList();
                    queues.Insert(0, options.ServerName);
                    options.Queues = queues.ToArray();
                }

                if (configAction != null)
                    configAction(options);

                storage = storage ?? provider.GetService<JobStorage>() ?? JobStorage.Current;
                additionalProcesses = additionalProcesses ?? provider.GetServices<IBackgroundProcess>();

                options.Activator = options.Activator ?? provider.GetService<JobActivator>();
                options.FilterProvider = options.FilterProvider ?? provider.GetService<IJobFilterProvider>();
                options.TimeZoneResolver = options.TimeZoneResolver ?? provider.GetService<ITimeZoneResolver>();

                GetInternalServices(provider, out var factory, out var stateChanger, out var performer);

#pragma warning disable 618
                return new BackgroundJobServerHostedService(
#pragma warning restore 618
                    storage, options, additionalProcesses, factory, performer, stateChanger);
            });
        }

        public static IServiceCollection AddHangfireServerServices(this IServiceCollection services, Action<BackgroundJobServerOptions> configAction = null, JobStorage storage = null)
        {
            var options = new BackgroundJobServerOptions();

            if (configAction != null)
                configAction(options);

            services.AddSingleton<IBackgroundJobClient>(x =>
            {
                ThrowIfNotConfigured(x);

                if (GetInternalServices(x, out var factory, out var stateChanger, out _))
                {
                    return new BackgroundJobClient(storage ?? x.GetRequiredService<JobStorage>(), factory, stateChanger);
                }

                return new BackgroundJobClient(
                    storage ?? x.GetRequiredService<JobStorage>(),
                    options.FilterProvider ?? x.GetRequiredService<IJobFilterProvider>());
            });

            services.AddSingleton<IRecurringJobManager>(x =>
            {
                ThrowIfNotConfigured(x);

                if (GetInternalServices(x, out var factory, out _, out _))
                {
                    return new RecurringJobManager(
                        storage ?? x.GetRequiredService<JobStorage>(),
                        factory,
                         options.TimeZoneResolver ?? x.GetService<ITimeZoneResolver>());
                }

                return new RecurringJobManager(
                   storage ?? x.GetRequiredService<JobStorage>(),
                    options.FilterProvider ?? x.GetRequiredService<IJobFilterProvider>(),
                    options.TimeZoneResolver ?? x.GetService<ITimeZoneResolver>());
            });

            return services;
        }

        internal static void ThrowIfNotConfigured(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetService<IGlobalConfiguration>();
            if (configuration == null)
            {
                throw new InvalidOperationException(
                    "Unable to find the required services. Please add all the required services by calling 'IServiceCollection.AddHangfire' inside the call to 'ConfigureServices(...)' in the application startup code.");
            }
        }

        internal static bool GetInternalServices(
           IServiceProvider provider,
           out IBackgroundJobFactory factory,
           out IBackgroundJobStateChanger stateChanger,
           out IBackgroundJobPerformer performer)
        {
            factory = provider.GetService<IBackgroundJobFactory>();
            performer = provider.GetService<IBackgroundJobPerformer>();
            stateChanger = provider.GetService<IBackgroundJobStateChanger>();

            if (factory != null && performer != null && stateChanger != null)
            {
                return true;
            }

            factory = null;
            performer = null;
            stateChanger = null;

            return false;
        }

        public static IServiceCollection AddHangfireJob<HangfireJob>(this IServiceCollection services)
            where HangfireJob : class
        {
            return services.AddTransient<HangfireJob>();
        }
    }
}
