using Microsoft.Extensions.DependencyInjection;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServiceDiscovery(this IServiceCollection services)
        {
            services.AddScoped<IServiceDiscoveryClient, ServiceDiscoveryClient>();
            services.AddScoped<IServiceDiscoveryQuery, ServiceDiscoveryQuery>();
        }
    }
}