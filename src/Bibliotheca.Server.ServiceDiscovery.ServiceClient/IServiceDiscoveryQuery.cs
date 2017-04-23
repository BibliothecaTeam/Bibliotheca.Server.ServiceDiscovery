using System.Collections.Generic;
using System.Threading.Tasks;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public interface IServiceDiscoveryQuery
    {
        Task<IList<ServiceInformation>> GetServicesAsync(ServerOptions serverOptions);
        
        Task<ServiceInformation> GetServiceAsync(ServerOptions serverOptions, string serviceId);

        Task<ServiceInformation> GetServiceAsync(ServerOptions serverOptions, string[] tags);

        Task<IList<ServiceInformation>> GetServicesAsync(ServerOptions serverOptions, string[] tags);

        Task<IList<ServiceHealth>> GetServicesHealthAsync(ServerOptions serverOptions, string serviceName);
    }
}