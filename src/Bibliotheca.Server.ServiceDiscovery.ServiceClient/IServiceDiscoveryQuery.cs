using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public interface IServiceDiscoveryQuery
    {
        Task<IList<ServiceInformation>> GetServices(Action<ServerOptions> actionOptions);
        
        Task<IList<ServiceInformation>> GetServices(Action<ServerOptions> actionOptions, string serviceId);

        Task<IList<ServiceInformation>> GetServices(Action<ServerOptions> actionOptions, string[] tags);
    }
}