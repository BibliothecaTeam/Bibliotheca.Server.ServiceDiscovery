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
        private IList<ServiceInformation> _services;

        [Scenario("Service discovery application should return information about registered services when get service by Id")]
        public async Task ServiceDiscoveryApplicationShouldReturnInformationAboutRegisteredServicesWhenGetServiceById()
        {
            GivenServiceDiscoveryApplicationIsUpAndRunning();
            GivenApplicationIsRegistered("fake-getbyids-id1");
            GivenApplicationIsRegistered("fake-getbyids-id1");
            GivenApplicationIsRegistered("fake-getbyids-id2");
            GivenApplicationIsRegistered("fake-getbyids-id3");
            await WhenUserGetInformationAboutService("fake-getbyids-id1");
            ThenTwoApplicationsWithPrefixIdShouldBeReturned("fake-getbyids-id1");
        }

        [Given("Service discovery application is up and running")]
        private void GivenServiceDiscoveryApplicationIsUpAndRunning()
        {
        }

        [Given("Application is registered")]
        private void GivenApplicationIsRegistered(string serviceId)
        {
            var serviceDiscovery = new ServiceDiscoveryClient(new FakeMemoryCache());
            serviceDiscovery.Register((options) =>
            {
                options.ServiceOptions.Id = serviceId;
                options.ServiceOptions.Name = "Fake name";
                options.ServiceOptions.Address = "127.0.0.1";
                options.ServiceOptions.Port = 5555;
                options.ServiceOptions.HttpHealthCheck = string.Empty;
                options.ServiceOptions.Tags = new List<string>();
                options.ServerOptions.Address = "http://127.0.0.1:8500";
            });
        }

        [When("User get information about service")]
        private async Task WhenUserGetInformationAboutService(string serviceId)
        {
            var serviceQuery = new ServiceDiscoveryQuery();
            _services = await serviceQuery.GetServices((o) => o.Address = "http://127.0.0.1:8500", serviceId);
        }

        [Then("Two applications with prefix id should be returned")]
        private void ThenTwoApplicationsWithPrefixIdShouldBeReturned(string serviceId)
        {
            Assert.Equal(2, _services.Count);
            Assert.True(_services.Any(x => x.ID.StartsWith(serviceId)));
        }
    }
}