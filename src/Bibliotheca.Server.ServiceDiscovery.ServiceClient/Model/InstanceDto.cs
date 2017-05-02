namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model
{
    public class InstanceDto
    {
        public string Id { get; set; }

        public string Address { get; set; }

        public int Port { get; set; }

        public HealthStatusEnumDto HealthStatus { get; set; }

        public string HealthOuptput { get; set; }

        public string[] Tags { get; set; }

        public string Notes { get; set; }
    }
}