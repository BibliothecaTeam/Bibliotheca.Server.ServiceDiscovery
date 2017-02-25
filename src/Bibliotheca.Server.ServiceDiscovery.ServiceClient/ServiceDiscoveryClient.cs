using System;
using System.Linq;
using System.Net;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Exceptions;
using Consul;
using Microsoft.Extensions.Logging;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class ServiceDiscoveryClient : IServiceDiscoveryClient
    {
        private readonly ILogger _logger;

        public ServiceDiscoveryClient(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger<ServiceDiscoveryClient>();
        }

        public void Register(ServiceDiscoveryOptions serviceDiscoveryOptions)
        {
            ValidateOptions(serviceDiscoveryOptions);

            var agentServiceRegistration = CreateAgentServiceRegistration(serviceDiscoveryOptions.ServiceOptions);
            var registerServiceEventArgument = new RegisterServiceEventArgument
            {
                AgentServiceRegistration = agentServiceRegistration,
                ServerOptions = serviceDiscoveryOptions.ServerOptions
            };

            RegisterService(registerServiceEventArgument);
        }

        private void RegisterService(RegisterServiceEventArgument argument)
        {
            try
            {
                _logger?.LogInformation("Registering service in service discovery application...");

                using (var client = new ConsulClient((configuration) => {
                    configuration.Address = new Uri(argument.ServerOptions.Address);
                }))
                {
                    if (!IsServiceAlreadyRegistered(client, argument.AgentServiceRegistration.ID))
                    {
                        _logger?.LogInformation($"Sending information about service to service discovery application [address: {argument.AgentServiceRegistration.Address}, port: {argument.AgentServiceRegistration.Port}]...");
                        var writeResult = client.Agent.ServiceRegister(argument.AgentServiceRegistration).GetAwaiter().GetResult();

                        if (!RegistrationWasSuccessfull(writeResult))
                        {
                            throw new ServiceDiscoveryResponseException("Registration in service discovery application failed.");
                        }

                        _logger?.LogInformation("Registration in service discovery application complete.");
                    }
                }

            }
            catch (ServiceDiscoveryResponseException exception)
            {
                _logger?.LogWarning("Service discovery application is not running.");
                _logger?.LogWarning($"Exception ({exception.GetType()}): {exception.Message}");
            }
            catch (Exception exception)
            {
                _logger?.LogWarning("Unexpected exception from service discovery application.");
                _logger?.LogWarning($"Exception ({exception.GetType()}): {exception.Message}");
            }
        }

        private static bool RegistrationWasSuccessfull(WriteResult writeResult)
        {
            return writeResult.StatusCode == HttpStatusCode.Created || writeResult.StatusCode == HttpStatusCode.OK;
        }

        private bool IsServiceAlreadyRegistered(ConsulClient client, string serviceId)
        {
            var services = client.Agent.Services().GetAwaiter().GetResult();
            if (services.Response.Any(x => x.Value.ID == serviceId))
            {
                _logger?.LogInformation("Service is already registered in service discovery application.");
                return true;
            }

            _logger?.LogInformation("Service is not registered in service discovery application.");
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
            var agentServiceRegistration = new AgentServiceRegistration();
            agentServiceRegistration.Address = serviceOptions.Address;
            agentServiceRegistration.Port = serviceOptions.Port;
            agentServiceRegistration.ID = serviceOptions.Id;
            agentServiceRegistration.Name = serviceOptions.Name;
            agentServiceRegistration.Tags = serviceOptions.Tags.ToArray();

            agentServiceRegistration.Check = new AgentServiceCheck();
            agentServiceRegistration.Check.Interval = TimeSpan.FromSeconds(10);
            agentServiceRegistration.Check.Timeout = TimeSpan.FromSeconds(2);
            agentServiceRegistration.Check.HTTP = serviceOptions.HttpHealthCheck;

            return agentServiceRegistration;
        }
    }
}