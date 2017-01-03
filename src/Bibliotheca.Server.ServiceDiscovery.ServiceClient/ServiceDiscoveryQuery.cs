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
        private const int _randomSuffixLength = 7;

        public async Task<IList<ServiceInformation>> GetServices(Action<ServerOptions> actionOptions)
        {
            var serverOptions = new ServerOptions();
            actionOptions?.Invoke(serverOptions);

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

        public async Task<IList<ServiceInformation>> GetServices(Action<ServerOptions> actionOptions, string serviceId)
        {
            int idLength = serviceId.Length + _randomSuffixLength;
            var services = await GetServices(actionOptions);
            return services.Where(x => x.ID.Length == idLength && x.ID.StartsWith(serviceId)).ToList();
        }

        public async Task<IList<ServiceInformation>> GetServices(Action<ServerOptions> actionOptions, string[] tags)
        {
            var services = await GetServices(actionOptions);
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