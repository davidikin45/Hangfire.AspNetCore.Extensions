using Microsoft.AspNetCore.Builder;
#if NETCOREAPP3_0
using Microsoft.AspNetCore.Routing;
#endif
using System;

namespace Hangfire.AspNetCore.Extensions
{
    public static class HangfireEndpointRouteBuilderExtensions
    {
#if NETCOREAPP3_0
        public static IEndpointConventionBuilder MapHangfireDashboard(this IEndpointRouteBuilder endpoints, string route = "/hangfire", Action<DashboardOptions> configAction = null, JobStorage storage = null)
        {
            var requestHandler = endpoints.CreateApplicationBuilder().UseHangfireDashboard(route, configAction, storage).Build();
            return endpoints.Map(route + "/{**path}", requestHandler);
        }
#endif
    }
}
