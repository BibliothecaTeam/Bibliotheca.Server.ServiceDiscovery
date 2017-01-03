using System;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public interface IServiceDiscoveryClient
    {
        void Register(Action<ServiceDiscoveryOptions> actionOptions);
    }
}