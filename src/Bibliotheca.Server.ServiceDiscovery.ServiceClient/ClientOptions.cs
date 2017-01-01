namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class ClientOptions
    {
        public string ServiceId { get; set; }

        public string ServiceName { get; set; }

        public string AgentAddress { get; set; }

        public string Datacenter { get; set; }

        public string ClientAddres { get; set; }

        public int ClientPort { get; set; }
    }
}