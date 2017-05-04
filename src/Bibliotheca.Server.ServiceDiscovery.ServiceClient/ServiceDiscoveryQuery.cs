using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Exceptions;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model;
using Consul;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class ServiceDiscoveryQuery : IServiceDiscoveryQuery
    {
        public async Task<IList<ServiceDto>> GetServicesAsync(ServerOptions serverOptions)
        {
            var services = await GetServicesInformationAsync(serverOptions);
            var servicesDtos = await GetServiceDtos(serverOptions, services);
            return servicesDtos;
        }

        public async Task<IList<ServiceDto>> GetServicesAsync(ServerOptions serverOptions, string serviceName)
        {
            var services = await GetServicesInformationAsync(serverOptions, serviceName);
            var servicesDtos = await GetServiceDtos(serverOptions, services);
            return servicesDtos;
        }

        public async Task<IList<ServiceDto>> GetServicesAsync(ServerOptions serverOptions, string[] tags)
        {
            var services = await GetServicesInformationAsync(serverOptions, tags);
            var servicesDtos = await GetServiceDtos(serverOptions, services);
            return servicesDtos;
        }

        public async Task<InstanceDto> GetServiceInstanceAsync(ServerOptions serverOptions, string[] tags)
        {
            var services = await GetServicesInformationAsync(serverOptions, tags);
            var servicesDtos = await GetServiceDtos(serverOptions, services);

            var passesInstnces = new List<InstanceDto>();
            foreach(var service in servicesDtos)
            {
                var instances = service.Instances.Where(x => x.HealthStatus == HealthStatusEnumDto.Passing);
                passesInstnces.AddRange(instances);
            }

            if(passesInstnces.Count == 0)
            {
                return null;
            }

            var random = new Random();
            var index = random.Next(0, passesInstnces.Count - 1);
            return passesInstnces[index];
        }

        private async Task<List<ServiceDto>> GetServiceDtos(ServerOptions serverOptions, IList<ServiceInformationDto> services)
        {
            var servicesDtos = new List<ServiceDto>();

            var serviceNames = services.Select(x => x.Service).Distinct();
            foreach (var serviceName in serviceNames)
            {
                var serviceDto = new ServiceDto
                {
                    Name = serviceName
                };

                var servicesHealth = await GetServicesHealthAsync(serverOptions, serviceName);
                var instances = services.Where(x => x.Service == serviceName);

                foreach (var instance in instances)
                {
                    var instanceDto = new InstanceDto
                    {
                        Address = instance.Address,
                        Id = instance.ID,
                        Port = instance.Port,
                        Tags = instance.Tags
                    };

                    var healthStatus = servicesHealth.FirstOrDefault(x => x.ServiceID == instance.ID);
                    if (healthStatus != null)
                    {
                        instanceDto.HealthStatus = healthStatus.Status == "passing" ? HealthStatusEnumDto.Passing : HealthStatusEnumDto.Critical;
                        instanceDto.HealthOuptput = healthStatus.Output;
                        instanceDto.Notes = healthStatus.Notes;
                    }

                    serviceDto.Instances.Add(instanceDto);
                }

                servicesDtos.Add(serviceDto);
            }

            return servicesDtos;
        }

        private async Task<IList<ServiceInformationDto>> GetServicesInformationAsync(ServerOptions serverOptions)
        {
            var client = new ConsulClient((options) =>
            {
                options.Address = new Uri(serverOptions.Address);
            });

            var services = await client.Agent.Services();
            if(services.StatusCode != HttpStatusCode.OK)
            {
                throw new ServiceDiscoveryResponseException("Exception during request to service discovery.");
            }

            return services.Response.Select(x => MapToServiceInformationDto(x.Value)).ToList();
        }

        private async Task<IList<ServiceInformationDto>> GetServicesInformationAsync(ServerOptions serverOptions, string[] tags)
        {
            var services = await GetServicesInformationAsync(serverOptions);
            return services.Where(x => x.Tags.Intersect(tags).Any()).ToList();
        }

        private async Task<IList<ServiceInformationDto>> GetServicesInformationAsync(ServerOptions serverOptions, string serviceName)
        {
            var services = await GetServicesInformationAsync(serverOptions);
            return services.Where(x => x.Service == serviceName).ToList();
        }

        private async Task<IList<ServiceHealthDto>> GetServicesHealthAsync(ServerOptions serverOptions, string serviceName)
        {
            var client = new ConsulClient((options) =>
            {
                options.Address = new Uri(serverOptions.Address);
            });

            var servicesHealth = await client.Health.Service(serviceName);
            if(servicesHealth.StatusCode != HttpStatusCode.OK)
            {
                throw new ServiceDiscoveryResponseException("Exception during request to service discovery.");
            }

            var healthDtos = new List<ServiceHealthDto>();
            foreach(var node in servicesHealth.Response)
            {
                if(node.Checks == null)
                {
                    continue;
                }

                foreach(var check in node.Checks)
                {
                    if(check.ServiceName == serviceName)
                    {
                        var serviceHealth = MapToServiceHealthDto(check);
                        healthDtos.Add(serviceHealth);
                    }
                }
            }

            return healthDtos;
        }

        private ServiceInformationDto MapToServiceInformationDto(AgentService agentService)
        {
            return new ServiceInformationDto
            {
                Address = agentService.Address,
                EnableTagOverride = agentService.EnableTagOverride,
                ID = agentService.ID,
                Port = agentService.Port,
                Service = agentService.Service,
                Tags = agentService.Tags
            };
        }

        private ServiceHealthDto MapToServiceHealthDto(HealthCheck healthCheck)
        {
            return new ServiceHealthDto
            {
                Node = healthCheck.Node,
                CheckID = healthCheck.CheckID,
                Name = healthCheck.Name,
                Status = healthCheck.Status?.Status,
                Notes = healthCheck.Notes,
                Output = healthCheck.Output,
                ServiceID = healthCheck.ServiceID,
                ServiceName = healthCheck.ServiceName
            };
        }
    }
}