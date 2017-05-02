namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model
{
    public class ServiceInformationDto
    {
        public string Address { get; set; }

        public bool EnableTagOverride { get; set; }

        public string ID { get; set; }

        public int Port { get; set; }

        public string Service { get; set; }
        
        public string[] Tags { get; set; }
    }
}