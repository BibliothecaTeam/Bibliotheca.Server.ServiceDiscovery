using FluentBehave;
using System.Collections.Generic;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Specs.Implementations
{
    [Feature("ServiceDiscoveryQueryGetServiceByTags", "Getting service by tags have to return correct information about service")]
    public class ServiceDiscoveryQueryGetServiceByTags
    {
        private IList<ServiceInformation> _services;

        [Scenario("Service discovery application should return information about registered service when user get service by tag")]
        public async Task ServiceDiscoveryApplicationShouldReturnInformationAboutRegisteredServicesWhenUserGetServiceByTag()
        {
            GivenServiceDiscoveryApplicationIsUpAndRunning();
            await GivenApplicationIsRegisteredWithTag("fake-getbytag-id1", "tag1");
            await GivenApplicationIsRegisteredWithTag("fake-getbytag-id2", "tag1");
            await GivenApplicationIsRegisteredWithTag("fake-getbytag-id3", "tag2");
            await GivenApplicationIsRegisteredWithTag("fake-getbytag-id4", "tag3");
            await WhenUserGetInformationAboutServicesWithTag("tag1");
            ThenApplicationWithIdWasReturned("fake-getbytag-id1", "fake-getbytag-id2");
        }

        [Given("Service discovery application is up and running")]
        private void GivenServiceDiscoveryApplicationIsUpAndRunning()
        {
        }

        [Given("Application is registered with tag")]
        private async Task GivenApplicationIsRegisteredWithTag(string serviceId, string tag)
        {
            var options = new ServiceDiscoveryOptions();
            options.ServiceOptions.Id = serviceId;
            options.ServiceOptions.Name = "Fake name";
            options.ServiceOptions.Address = "10.1.1.1";
            options.ServiceOptions.Port = 5555;
            options.ServiceOptions.HttpHealthCheck = string.Empty;
            options.ServiceOptions.Tags = new List<string>() { tag };
            options.ServerOptions.Address = "http://127.0.0.1:8500";

            var serviceDiscovery = new ServiceDiscoveryClient(null);
            await serviceDiscovery.RegisterAsync(options);
        }

        [When("User get information about services with tag")]
        private async Task WhenUserGetInformationAboutServicesWithTag(string tag)
        {
            var serviceQuery = new ServiceDiscoveryQuery();
            _services = await serviceQuery.GetServicesAsync(new ServerOptions { Address = "http://127.0.0.1:8500" }, new string[] { tag });
        }

        [Then("Application with id was returned")]
        private void ThenApplicationWithIdWasReturned(string serviceId1, string serviceId2)
        {
            Assert.True(_services.Any(x => x.ID == serviceId1 || x.ID == serviceId2));
        }
    }
}