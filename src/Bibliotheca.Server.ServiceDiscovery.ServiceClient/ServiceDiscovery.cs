using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Exceptions;
using Consul;
using Microsoft.Extensions.Configuration;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class ServiceDiscovery
    {
        public void Register(IConfigurationRoot configuration)
        {
            CatalogRegistration catalogRegister = CreateCatalogRegistration(configuration);
            WriteResult writeResult = null;
            try
            {
                var client = new ConsulClient();
                writeResult = client.Catalog.Register(catalogRegister).GetAwaiter().GetResult();
            }
            catch
            {
                throw new ServiceDiscoveryRegistrationException("Exception during registration node.");
            }

            if (writeResult.StatusCode != HttpStatusCode.Created && writeResult.StatusCode != HttpStatusCode.OK)
            {
                throw new ServiceDiscoveryResponseException("Registration failed.");
            }
        }

        private CatalogRegistration CreateCatalogRegistration(IConfigurationRoot configuration)
        {
            var serviceDiscoverySection = configuration.GetSection("ServiceDiscovery");
            var serviceId = serviceDiscoverySection["ServiceId"];
            var serviceName = serviceDiscoverySection["ServiceName"];
            var agentAddress = serviceDiscoverySection["AgentAddress"];
            var datacenter = serviceDiscoverySection["Datacenter"];

            var randomNodeId = GenerateRandomNode(6);
            var nodeId = $"{serviceId} - node {randomNodeId}";

            var catalogRegister = new CatalogRegistration();
            catalogRegister.Datacenter = datacenter;
            catalogRegister.Node = nodeId;
            catalogRegister.Address = agentAddress;

            var localAddress = GetLocalAddress();
            var localPort = GetPort(configuration);

            catalogRegister.Service = new AgentService();
            catalogRegister.Service.ID = serviceId;
            catalogRegister.Service.Service = serviceName;
            catalogRegister.Service.Address = localAddress;
            catalogRegister.Service.Port = localPort;

            catalogRegister.Check = new AgentCheck();
            catalogRegister.Check.Node = nodeId;
            catalogRegister.Check.CheckID = $"service:{serviceId}";
            catalogRegister.Check.Name = $"{serviceName} Health Check";
            catalogRegister.Check.Notes = "Script based health check";
            catalogRegister.Check.Status = CheckStatus.Passing;
            catalogRegister.Check.ServiceID = serviceId;
            return catalogRegister;
        }

        private int GetPort(IConfigurationRoot configuration)
        {
            var address = configuration["server.urls"];
            if (!string.IsNullOrWhiteSpace(address))
            {
                var url = new Uri(address);
                return url.Port;
            }

            return 5000;
        }

        private string GetLocalAddress()
        {
            var hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddressesAsync(hostName).GetAwaiter().GetResult();
            foreach (var ip in addresses)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return string.Empty;
        }

        private string GenerateRandomNode(int length)
        {
            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}