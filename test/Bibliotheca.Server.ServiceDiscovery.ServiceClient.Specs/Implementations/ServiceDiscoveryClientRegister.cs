using FluentBehave;
using System;
using System.Collections.Generic;
using Xunit;
using Consul;
using System.Threading.Tasks;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Specs.Implementations
{
    [Feature("ServiceDiscoveryClientRegister", "Registering a client in service discovery application")]
    public class ServiceDiscoveryClientRegister
    {
        private bool _result;

        [Scenario("Client should be successfully registered when I specify correct options")]
        public async Task ClientShouldBeSuccessfullyRegisteredWhenISpecifyCorrectOptions()
        {
            GivenServiceDiscoveryApplicationIsUpAndRunning();
            WhenUserRegisterApplicationWithIdAndAddress("fake-app-id", "10.1.1.2");
            ThenDiscoveryServiceApplicationReturnsStatusCodeOk();
            await ThenApplicationWithIdAndAddressIsSuccessfullyRegistered("fake-app-id", "10.1.1.2");
        }

        [Given("Service discovery application is up and running")]
        private void GivenServiceDiscoveryApplicationIsUpAndRunning()
        {
        }

        [When("User register application with id and address")]
        private void WhenUserRegisterApplicationWithIdAndAddress(string serviceId, string serviceAddress)
        {
            try
            {
                var serviceDiscovery = new ServiceDiscoveryClient(new FakeMemoryCache());
                serviceDiscovery.Register((options) => 
                {
                    options.ServiceOptions.Id = serviceId;
                    options.ServiceOptions.Name = "Fake name";
                    options.ServiceOptions.Address = serviceAddress;
                    options.ServiceOptions.Port = 5555;
                    options.ServiceOptions.HttpHealthCheck = string.Empty;
                    options.ServiceOptions.Tags = new List<string>();
                    options.ServerOptions.Address = "http://127.0.0.1:8500";
                });

                _result = true;
            }
            catch (Exception)
            {
                _result = false;
            }
        }

        [Then("Discovery service application returns status code Ok")]
        private void ThenDiscoveryServiceApplicationReturnsStatusCodeOk()
        {
            Assert.True(_result);
        }

        [Then("Application with id and address is successfully registered")]
        private async Task ThenApplicationWithIdAndAddressIsSuccessfullyRegistered(string serviceId, string serviceAddress)
        {
            var client = new ConsulClient();
            var services = await client.Agent.Services();

            AgentService agentService = null;
            foreach(var item in services.Response)
            {
                if(item.Value.ID.StartsWith($"{serviceId}-"))
                {
                    agentService = item.Value;
                }
            }

            Assert.NotNull(agentService);
            Assert.Equal(serviceAddress, agentService.Address);
        }
    }
}