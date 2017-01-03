using System;
using System.Linq;
using System.Net;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Exceptions;
using Consul;
using Microsoft.Extensions.Caching.Memory;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class ServiceDiscoveryClient : IServiceDiscoveryClient
    {
        private const int _randomSuffixLength = 6;
        private readonly IMemoryCache _memoryCache;

        public ServiceDiscoveryClient(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Register(Action<ServiceDiscoveryOptions> actionOptions)
        {
            var options = new ServiceDiscoveryOptions();
            actionOptions?.Invoke(options);

            ValidateOptions(options);

            var agentServiceRegistration = CreateAgentServiceRegistration(options.ServiceOptions);
            var registerServiceEventArgument = new RegisterServiceEventArgument
            {
                AgentServiceRegistration = agentServiceRegistration,
                ServerOptions = options.ServerOptions
            };

            RegisterService(registerServiceEventArgument);
        }

        private void RegisterService(RegisterServiceEventArgument argument)
        {
            _memoryCache.Set(argument.AgentServiceRegistration.ID, argument,
                new MemoryCacheEntryOptions().RegisterPostEvictionCallback((key, value, reason, substate) =>
                {
                    if (reason == EvictionReason.Expired)
                    {
                        var options = value as RegisterServiceEventArgument;
                        RegisterService(options);
                    }
                }).SetAbsoluteExpiration(TimeSpan.FromMinutes(1))
            );

            WriteResult writeResult = null;
            try
            {
                var client = new ConsulClient((configuration) =>
                {
                    configuration.Address = new Uri(argument.ServerOptions.Address);
                });

                if (!IsServiceAlreadyRegistered(client, argument.AgentServiceRegistration.ID))
                {
                    writeResult = client.Agent.ServiceRegister(argument.AgentServiceRegistration).GetAwaiter().GetResult();

                    if (writeResult.StatusCode != HttpStatusCode.Created && writeResult.StatusCode != HttpStatusCode.OK)
                    {
                        throw new ServiceDiscoveryResponseException("Registration failed.");
                    }
                }

            }
            catch (Exception exception)
            {
                throw new ServiceDiscoveryRegistrationException($"Exception during registration node: {exception.Message}");
            }
        }

        private bool IsServiceAlreadyRegistered(ConsulClient client, string serviceId)
        {
            var services = client.Agent.Services().GetAwaiter().GetResult();
            if (services.Response.Any(x => x.Value.ID == serviceId))
            {
                return true;
            }

            return false;
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
            var randomNodeId = GenerateRandomNode(_randomSuffixLength);
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