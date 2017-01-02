namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class ServiceDiscoveryOptions
    {
        public ServiceDiscoveryOptions()
        {
            ServiceOptions = new ServiceOptions();
            ServerOptions = new ServerOptions();
        }

        public ServiceOptions ServiceOptions { get; private set; }

        public ServerOptions ServerOptions { get; private set; }
    }
}