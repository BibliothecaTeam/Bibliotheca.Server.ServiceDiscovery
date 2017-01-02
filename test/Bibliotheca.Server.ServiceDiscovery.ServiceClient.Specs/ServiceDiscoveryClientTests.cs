using System;
using System.Collections.Generic;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient;
using Xunit;

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void ServiceClientHaveToExecuteWithCorrectConfiguration()
        {
            try
            {
                var serviceDiscovery = new ServiceDiscoveryClient();
                serviceDiscovery.Register((options) => 
                {
                    options.ServiceOptions.Id = "fake-id";
                    options.ServiceOptions.Name = "Fake name";
                    options.ServiceOptions.Address = "127.9.9.9";
                    options.ServiceOptions.Port = 5555;
                    options.ServiceOptions.HttpHealthCheck = string.Empty;
                    options.ServiceOptions.Tags = new List<string>(){ "tag1", "tag2" };
                    options.ServerOptions.Address = "http://127.0.0.1:8500";
                });
            }
            catch (Exception exception)
            {
                Assert.True(false, exception.Message);
            }
        }
    }
}
