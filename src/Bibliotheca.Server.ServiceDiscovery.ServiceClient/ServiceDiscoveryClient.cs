using System;
using System.Linq;
using System.Net;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Exceptions;
using Consul;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class ServiceDiscoveryClient
    {
        public void Register(Action<ServiceDiscoveryOptions> actionOptions)
        {
            var options = new ServiceDiscoveryOptions();
            actionOptions?.Invoke(options);

            ValidateOptions(options);

            var agentServiceRegistration = CreateAgentServiceRegistration(options.ServiceOptions);
            WriteResult writeResult = null;
            try
            {
                var client = new ConsulClient((configuration) =>
                {
                    configuration.Address = new Uri(options.ServerOptions.Address);
                });

                writeResult = client.Agent.ServiceRegister(agentServiceRegistration).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                throw new ServiceDiscoveryRegistrationException($"Exception during registration node: {exception.Message}");
            }

            if (writeResult.StatusCode != HttpStatusCode.Created && writeResult.StatusCode != HttpStatusCode.OK)
            {
                throw new ServiceDiscoveryResponseException("Registration failed.");
            }
        }

        private void ValidateOptions(ServiceDiscoveryOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ServiceOptions.Id))
            {
                throw new ServiceIdNotDeliveredException();
            }

            if (string.IsNullOrWhiteSpace(options.ServiceOptions.Name))
            {
                throw new ServiceNameNotDeliveredException();
            }

            if (string.IsNullOrWhiteSpace(options.ServiceOptions.Address))
            {
                throw new ServiceAddressNotDeliveredException();
            }

            if (options.ServiceOptions.Port == 0)
            {
                throw new ServicePortNotDeliveredException();
            }

            if (string.IsNullOrWhiteSpace(options.ServerOptions.Address))
            {
                throw new ServerAddressNotDeliveredException();
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
            agentServiceRegistration.Tags = serviceOptions.Tags.ToArray();

            agentServiceRegistration.Check = new AgentServiceCheck();
            agentServiceRegistration.Check.Interval = TimeSpan.FromSeconds(10);
            agentServiceRegistration.Check.Timeout = TimeSpan.FromSeconds(2);
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