using Hangfire.Server;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.AspNetCore.Extensions
{
    public static class HangfireApplicationBuilderExtensions
    {
        /// <summary>
        /// Exposes hangfire dashboard.
        /// </summary>
        public static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder builder, string route = "/hangfire", Action<DashboardOptions> configAction = null, JobStorage storage = null)
        {
            var options = new DashboardOptions
            {
                //must be set otherwise only local access allowed
                //Authorization = new[] { new HangfireRoleAuthorizationfilter() },
                AppPath = route.Replace("/hangfire", "")
            };

            if (configAction != null)
                configAction(options);

            builder.UseHangfireDashboard(route, options, storage);

            return builder;
        }

        /// <summary>
        /// Starts the hangfire server. Better to use AddHangfireServer.
        /// </summary>
        public static IApplicationBuilder UseHangfireServer(this IApplicationBuilder builder, string serverName, IEnumerable<IBackgroundProcess> additionalProcesses = null, JobStorage storage = null)
        {
            //each microserver has its own queue. Queue by using the Queue attribute.
            //https://discuss.hangfire.io/t/one-queue-for-the-whole-farm-and-one-queue-by-server/490
            var options = new BackgroundJobServerOptions
            {
                ServerName = serverName
                //Queues = new[] { serverName, EnqueuedState.DefaultQueue }
            };

            if (!string.IsNullOrEmpty(options.ServerName) && !options.Queues.Contains(options.ServerName))
            {
                var queues = options.Queues.ToList();
                queues.Insert(0, options.ServerName);
                options.Queues = queues.ToArray();
            }

            //https://discuss.hangfire.io/t/one-queue-for-the-whole-farm-and-one-queue-by-server/490/3

            builder.UseHangfireServer(options, additionalProcesses, storage);
            return builder;
        }

    }
}
