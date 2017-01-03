using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Exceptions;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model;
using Consul;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class ServiceDiscoveryQuery : IServiceDiscoveryQuery
    {
        public async Task<IList<ServiceInformation>> GetServices(ServerOptions serverOptions)
        {
            var client = new ConsulClient((options) =>
            {
                options.Address = new Uri(serverOptions.Address);
            });

            var services = await client.Agent.Services();
            if(services.StatusCode != HttpStatusCode.OK)
            {
                throw new ServiceDiscoveryResponseException("Exception during request to service discovery.");
            }

            return services.Response.Select(x => MapToServiceInformation(x.Value)).ToList();
        }

        public async Task<ServiceInformation> GetService(ServerOptions serverOptions, string serviceId)
        {
            var services = await GetServices(serverOptions);
            return services.FirstOrDefault(x => x.ID == serviceId);
        }

        public async Task<IList<ServiceInformation>> GetServices(ServerOptions serverOptions, string[] tags)
        {
            var services = await GetServices(serverOptions);
            return services.Where(x => x.Tags.Intersect(tags).Any()).ToList();
        }

        private ServiceInformation MapToServiceInformation(AgentService agentService)
        {
            return new ServiceInformation
            {
                Address = agentService.Address,
                EnableTagOverride = agentService.EnableTagOverride,
                ID = agentService.ID,
                Port = agentService.Port,
                Service = agentService.Service,
                Tags = agentService.Tags
            };
        }
    }
}