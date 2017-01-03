namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public interface IServiceDiscoveryClient
    {
        void Register(ServiceDiscoveryOptions options);
    }
}