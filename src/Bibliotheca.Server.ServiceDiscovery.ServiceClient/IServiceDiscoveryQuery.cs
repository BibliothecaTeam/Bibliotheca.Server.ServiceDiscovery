using System.Collections.Generic;
using System.Threading.Tasks;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public interface IServiceDiscoveryQuery
    {
        Task<IList<ServiceDto>> GetServicesAsync(ServerOptions serverOptions);

        Task<IList<ServiceDto>> GetServicesAsync(ServerOptions serverOptions, string serviceName);

        Task<IList<ServiceDto>> GetServicesAsync(ServerOptions serverOptions, string[] tags);

        Task<InstanceDto> GetServiceInstanceAsync(ServerOptions serverOptions, string[] tags);
    }
}