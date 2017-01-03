using System;
using Microsoft.AspNetCore.Builder;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void RegisterService(this IApplicationBuilder applicationBuilder, Action<ServiceDiscoveryOptions> actionOptions)
        {
            var service = applicationBuilder.ApplicationServices.GetService(typeof(IServiceDiscoveryClient)) as IServiceDiscoveryClient;

            service.Register(actionOptions);
        }
    }
}