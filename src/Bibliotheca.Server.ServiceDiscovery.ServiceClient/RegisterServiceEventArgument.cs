using Consul;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class RegisterServiceEventArgument
    {
        public AgentServiceRegistration AgentServiceRegistration { get; set; }

        public ServerOptions ServerOptions { get; set; }
    }
}