using System.Threading.Tasks;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient
{
    public interface IServiceDiscoveryClient
    {
        Task RegisterAsync(ServiceDiscoveryOptions options);
    }
}