using FluentBehave;
using System.Collections.Generic;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model;
using System.Threading.Tasks;
using System.Linq;
using Xunit;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Specs.Implementations
{
    [Feature("ServiceDiscoveryQueryGetAllServices", "Getting all services from service discovery application")]
    public class ServiceDiscoveryQueryGetAllServices
    {
        private IList<ServiceInformation> _services;

        [Scenario("Service discovery application should return information about registered services when user get all services")]
        public async Task ServiceDiscoveryApplicationShouldReturnInformationAboutRegisteredServicesWhenUserGetAllServices()
        {
            GivenServiceDiscoveryApplicationIsUpAndRunning();
            GivenApplicationIsRegistered("fake-getall-id1");
            GivenApplicationIsRegistered("fake-getall-id2");
            await WhenUserGetInformationAboutRegisteredServices();
            ThenApplicationWithIdExistsOnList("fake-getall-id1");
            ThenApplicationWithIdExistsOnList("fake-getall-id2");
        }

        [Given("Service discovery application is up and running")]
        private void GivenServiceDiscoveryApplicationIsUpAndRunning()
        {
        }

        [Given("Application is registered")]
        private void GivenApplicationIsRegistered(string serviceId)
        {
            var options = new ServiceDiscoveryOptions();
            options.ServiceOptions.Id = serviceId;
            options.ServiceOptions.Name = "Fake name";
            options.ServiceOptions.Address = "10.1.1.0";
            options.ServiceOptions.Port = 5555;
            options.ServiceOptions.HttpHealthCheck = string.Empty;
            options.ServiceOptions.Tags = new List<string>();
            options.ServerOptions.Address = "http://127.0.0.1:8500";

            var serviceDiscovery = new ServiceDiscoveryClient();
            serviceDiscovery.Register(options);
        }

        [When("User get information about registered services")]
        private async Task WhenUserGetInformationAboutRegisteredServices()
        {
            var serviceQuery = new ServiceDiscoveryQuery();
            _services = await serviceQuery.GetServices(new ServerOptions { Address = "http://127.0.0.1:8500" });
        }

        [Then("Application with id exists on list")]
        private void ThenApplicationWithIdExistsOnList(string serviceId)
        {
            Assert.True(_services.Any(x => x.ID == serviceId));
        }
    }
}