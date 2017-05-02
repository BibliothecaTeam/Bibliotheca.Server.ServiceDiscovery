using FluentBehave;
using System.Collections.Generic;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model;
using Xunit;
using System.Threading.Tasks;
using System.Linq;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Specs.Implementations
{
    [Feature("ServiceDiscoveryQueryGetServicesById", "Getting service by id have to return correct information about services")]
    public class ServiceDiscoveryQueryGetServicesById
    {
        private IList<ServiceDto> _services;

        [Scenario("Service discovery application should return information about registered services when get service by Id")]
        public async Task ServiceDiscoveryApplicationShouldReturnInformationAboutRegisteredServicesWhenGetServiceById()
        {
            GivenServiceDiscoveryApplicationIsUpAndRunning();
            await GivenApplicationIsRegistered("fake-getbyids-id1");
            await GivenApplicationIsRegistered("fake-getbyids-id2");
            await GivenApplicationIsRegistered("fake-getbyids-id3");
            await GivenApplicationIsRegistered("fake-getbyids-id4");
            await WhenUserGetInformationAboutService("fake-getbyids-id1");
            ThenApplicationWithIdShouldBeReturned("fake-getbyids-id1");
        }

        [Given("Service discovery application is up and running")]
        private void GivenServiceDiscoveryApplicationIsUpAndRunning()
        {
        }

        [Given("Application is registered")]
        private async Task GivenApplicationIsRegistered(string serviceId)
        {
                var options = new ServiceDiscoveryOptions();
                options.ServiceOptions.Id = serviceId;
                options.ServiceOptions.Name = "Fake name";
                options.ServiceOptions.Address = "10.1.1.1";
                options.ServiceOptions.Port = 5555;
                options.ServiceOptions.HttpHealthCheck = string.Empty;
                options.ServiceOptions.Tags = new List<string>();
                options.ServerOptions.Address = "http://127.0.0.1:8500";

                var serviceDiscovery = new ServiceDiscoveryClient(null);
                await serviceDiscovery.RegisterAsync(options);
        }

        [When("User get information about service")]
        private async Task WhenUserGetInformationAboutService(string serviceName)
        {
            var serviceQuery = new ServiceDiscoveryQuery();
            _services = await serviceQuery.GetServicesAsync(new ServerOptions { Address = "http://127.0.0.1:8500" }, serviceName);
        }

        [Then("Application with id should be returned")]
        private void ThenApplicationWithIdShouldBeReturned(string serviceId)
        {
            Assert.True(_services.SelectMany(x => x.Instances).Any(y => y.Id == serviceId));
        }
    }
}