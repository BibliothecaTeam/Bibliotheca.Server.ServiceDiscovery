namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class ServiceOptions
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public int Port { get; set; }

        public string HttpHealthCheck { get; set; }
    }
}