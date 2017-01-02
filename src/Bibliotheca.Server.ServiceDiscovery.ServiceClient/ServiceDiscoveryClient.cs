using System;
using System.Linq;
using System.Net;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Exceptions;
using Consul;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class ServiceDiscoveryClient
    {
        public void Register(ServiceOptions serviceOptions, ServerOptions serverOptions)
        {
            if (string.IsNullOrWhiteSpace(serviceOptions.Id))
            {
                throw new ServiceIdNotDeliveredException();
            }

            if (string.IsNullOrWhiteSpace(serviceOptions.Name))
            {
                throw new ServiceNameNotDeliveredException();
            }

            if (string.IsNullOrWhiteSpace(serviceOptions.Address))
            {
                throw new ServiceAddressNotDeliveredException();
            }

            if (serviceOptions.Port == 0)
            {
                throw new ServicePortNotDeliveredException();
            }

            if (string.IsNullOrWhiteSpace(serverOptions.Address))
            {
                throw new ServerAddressNotDeliveredException();
            }

            AgentServiceRegistration agentServiceRegistration = CreateAgentServiceRegistration(serviceOptions);
            WriteResult writeResult = null;
            try
            {
                var client = new ConsulClient((configuration) => 
                {
                    configuration.Address = new Uri(serverOptions.Address);
                });

                writeResult = client.Agent.ServiceRegister(agentServiceRegistration).GetAwaiter().GetResult();
            }
            catch(Exception exception)
            {
                throw new ServiceDiscoveryRegistrationException($"Exception during registration node: {exception.Message}" );
            }

            if (writeResult.StatusCode != HttpStatusCode.Created && writeResult.StatusCode != HttpStatusCode.OK)
            {
                throw new ServiceDiscoveryResponseException("Registration failed.");
            }
        }

        private AgentServiceRegistration CreateAgentServiceRegistration(ServiceOptions serviceOptions)
        {
            var randomNodeId = GenerateRandomNode(6);
            var serviceUniqueId = $"{serviceOptions.Id}-{randomNodeId}";

            var agentServiceRegistration = new AgentServiceRegistration();
            agentServiceRegistration.Address = serviceOptions.Address;
            agentServiceRegistration.Port = serviceOptions.Port;
            agentServiceRegistration.ID = serviceUniqueId;
            agentServiceRegistration.Name = serviceOptions.Name;

            agentServiceRegistration.Check = new AgentServiceCheck();
            agentServiceRegistration.Check.Interval = TimeSpan.FromSeconds(10);
            agentServiceRegistration.Check.TTL = TimeSpan.FromSeconds(15);
            agentServiceRegistration.Check.HTTP = serviceOptions.HttpHealthCheck;

            return agentServiceRegistration;
        }

        private string GenerateRandomNode(int length)
        {
            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}