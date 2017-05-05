using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Exceptions;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Model;
using Consul;
using Microsoft.Extensions.Caching.Memory;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public class ServiceDiscoveryQuery : IServiceDiscoveryQuery
    {
        private readonly IMemoryCache _memoryCache;

        public ServiceDiscoveryQuery(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

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
            List<InstanceDto> healthyInstnces = await GetHealthyInstances(serverOptions, tags);

            if (healthyInstnces.Count == 0)
            {
                return null;
            }

            if (healthyInstnces.Count == 1)
            {
                return healthyInstnces[0];
            }

            var instance = SelectRandomInstance(healthyInstnces);
            return instance;
        }

        private async Task<List<InstanceDto>> GetHealthyInstances(ServerOptions serverOptions, string[] tags)
        {
            List<InstanceDto> healthyInstnces = null;
            if(_memoryCache == null)
            {
                healthyInstnces = await DownloadHealthyInstances(serverOptions, tags, healthyInstnces);
            }
            else
            {
                var cacheKey = GetCacheKey(tags);
                if (!_memoryCache.TryGetValue(cacheKey, out healthyInstnces))
                {
                    healthyInstnces = await DownloadHealthyInstances(serverOptions, tags, healthyInstnces);

                    _memoryCache.Set(cacheKey, healthyInstnces,
                        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(30)));
                }
            }

            return healthyInstnces;
        }

        private string GetCacheKey(string[] tags)
        {
            var tag = string.Join("#", tags);
            return $"HealthyInstances#";
        }

        private async Task<List<InstanceDto>> DownloadHealthyInstances(ServerOptions serverOptions, string[] tags, List<InstanceDto> healthyInstnces)
        {
            var services = await GetServicesInformationAsync(serverOptions, tags);
            var servicesDtos = await GetServiceDtos(serverOptions, services);

            healthyInstnces = new List<InstanceDto>();
            foreach (var service in servicesDtos)
            {
                var instances = service.Instances.Where(x => x.HealthStatus == HealthStatusEnumDto.Passing);
                healthyInstnces.AddRange(instances);
            }

            return healthyInstnces;
        }

        private static InstanceDto SelectRandomInstance(List<InstanceDto> healthyInstnces)
        {
            var random = new Random();
            var index = random.Next(0, healthyInstnces.Count - 1);
            return healthyInstnces[index];
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
            QueryResult<Dictionary<string, AgentService>> services = null;
            using(var client = new ConsulClient((options) => { 
                options.Address = new Uri(serverOptions.Address); 
            }))
            {
                services = await client.Agent.Services();
            }

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
            QueryResult<ServiceEntry[]> servicesHealth = null;
            using(var client = new ConsulClient((options) => {
                options.Address = new Uri(serverOptions.Address);
            }))
            {
                servicesHealth = await client.Health.Service(serviceName);
            }
            
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