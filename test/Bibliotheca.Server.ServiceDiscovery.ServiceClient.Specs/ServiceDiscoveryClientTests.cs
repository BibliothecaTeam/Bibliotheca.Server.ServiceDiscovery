using System;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient;
using Xunit;

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void ServiceClientHaveToExecuteWithCorrectConfiguration()
        {
            var serviceOptions = new ServiceOptions
            {
                Id = "fake-id",
                Name = "Fake name",
                Address = "127.10.10.10",
                Port = 5000,
                HttpHealthCheck = ""
            };

            var serverOptions = new ServerOptions
            {
                Address = "http://127.0.0.1:8500"
            };

            var serviceDiscovery = new ServiceDiscoveryClient();

            try
            {
                serviceDiscovery.Register(serviceOptions, serverOptions);
            }
            catch (Exception exception)
            {
                Assert.True(false, exception.Message);
            }
        }
    }
}
