using System.Collections.Generic;
using System.Threading.Tasks;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public interface IServiceDiscoveryQuery
    {
        Task<IList<ServiceInformation>> GetServices(ServerOptions serverOptions);
        
        Task<ServiceInformation> GetService(ServerOptions serverOptions, string serviceId);

        Task<IList<ServiceInformation>> GetServices(ServerOptions serverOptions, string[] tags);
    }
}