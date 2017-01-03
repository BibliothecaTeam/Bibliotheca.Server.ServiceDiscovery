using FluentBehave;
using System.Collections.Generic;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Specs.Implementations
{
    [Feature("ServiceDiscoveryQueryGetServicesByTags", "Getting service by id have to return correct information about services")]
    public class ServiceDiscoveryQueryGetServicesByTags
    {
        private IList<ServiceInformation> _services;

        [Scenario("Service discovery application should return information about registered services when user get service by tag")]
        public async Task ServiceDiscoveryApplicationShouldReturnInformationAboutRegisteredServicesWhenUserGetServiceByTag()
        {
            GivenServiceDiscoveryApplicationIsUpAndRunning();
            GivenApplicationIsRegisteredWithTag("fake-getbytag-id1", "tag1");
            GivenApplicationIsRegisteredWithTag("fake-getbytag-id1", "tag1");
            GivenApplicationIsRegisteredWithTag("fake-getbytag-id2", "tag2");
            GivenApplicationIsRegisteredWithTag("fake-getbytag-id3", "tag3");
            await WhenUserGetInformationAboutServicesWithTag("tag1");
            ThenTwoApplicationsWithPrefixIdShouldBeReturned("fake-getbytag-id1");
        }

        [Given("Service discovery application is up and running")]
        private void GivenServiceDiscoveryApplicationIsUpAndRunning()
        {
        }

        [Given("Application is registered with tag")]
        private void GivenApplicationIsRegisteredWithTag(string serviceId, string tag)
        {
            var serviceDiscovery = new ServiceDiscoveryClient(new FakeMemoryCache());
            serviceDiscovery.Register((options) =>
            {
                options.ServiceOptions.Id = serviceId;
                options.ServiceOptions.Name = "Fake name";
                options.ServiceOptions.Address = "127.0.0.1";
                options.ServiceOptions.Port = 5555;
                options.ServiceOptions.HttpHealthCheck = string.Empty;
                options.ServiceOptions.Tags = new List<string>() { tag };
                options.ServerOptions.Address = "http://127.0.0.1:8500";
            });
        }

        [When("User get information about services with tag")]
        private async Task WhenUserGetInformationAboutServicesWithTag(string tag)
        {
            var serviceQuery = new ServiceDiscoveryQuery();
            _services = await serviceQuery.GetServices((o) => o.Address = "http://127.0.0.1:8500", 
                new string[] { tag });
        }

        [Then("Two applications with prefix id should be returned")]
        private void ThenTwoApplicationsWithPrefixIdShouldBeReturned(string serviceId)
        {
            Assert.Equal(2, _services.Count);
            Assert.True(_services.Any(x => x.ID.StartsWith(serviceId)));
        }
    }
}