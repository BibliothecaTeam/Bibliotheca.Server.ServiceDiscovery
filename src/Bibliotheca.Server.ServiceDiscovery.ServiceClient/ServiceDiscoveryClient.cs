using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Exceptions;
using Consul;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class ServiceDiscoveryClient
    {
        public void Register(ClientOptions clientOptions)
        {
            if(string.IsNullOrWhiteSpace(clientOptions.ServiceId))
            {
                throw new ServiceIdNotDeliveredException();
            }

            if(string.IsNullOrWhiteSpace(clientOptions.ServiceName))
            {
                throw new ServiceNameNotDeliveredException();
            }

            if(string.IsNullOrWhiteSpace(clientOptions.Datacenter))
            {
                throw new DatacenterNotDeliveredException();
            }

            if(string.IsNullOrWhiteSpace(clientOptions.AgentAddress))
            {
                throw new AgentAddressNotDeliveredException();
            }

            if(clientOptions.ClientPort == 0)
            {
                throw new ClientPortNotDeliveredException();
            }

            CatalogRegistration catalogRegister = CreateCatalogRegistration(clientOptions);
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

        private CatalogRegistration CreateCatalogRegistration(ClientOptions clientOptions)
        {
            var randomNodeId = GenerateRandomNode(6);
            var nodeId = $"{clientOptions.ServiceId} - node {randomNodeId}";

            var catalogRegister = new CatalogRegistration();
            catalogRegister.Datacenter = clientOptions.Datacenter;
            catalogRegister.Node = nodeId;
            catalogRegister.Address = clientOptions.AgentAddress;

            var localAddress = string.IsNullOrWhiteSpace(clientOptions.ClientAddres) 
                ? GetLocalAddress() : clientOptions.ClientAddres;
            var localPort = clientOptions.ClientPort;

            catalogRegister.Service = new AgentService();
            catalogRegister.Service.ID = clientOptions.ServiceId;
            catalogRegister.Service.Service = clientOptions.ServiceName;
            catalogRegister.Service.Address = localAddress;
            catalogRegister.Service.Port = localPort;

            catalogRegister.Check = new AgentCheck();
            catalogRegister.Check.Node = nodeId;
            catalogRegister.Check.CheckID = $"service:{clientOptions.ServiceId}";
            catalogRegister.Check.Name = $"{clientOptions.ServiceName} Health Check";
            catalogRegister.Check.Notes = "Script based health check";
            catalogRegister.Check.Status = CheckStatus.Passing;
            catalogRegister.Check.ServiceID = clientOptions.ServiceId;
            return catalogRegister;
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