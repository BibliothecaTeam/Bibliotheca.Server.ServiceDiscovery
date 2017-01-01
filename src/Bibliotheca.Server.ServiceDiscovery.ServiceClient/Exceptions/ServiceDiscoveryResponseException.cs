using System;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Exceptions
{
    public class ServiceDiscoveryResponseException : Exception
    {
        public ServiceDiscoveryResponseException(string message) : base(message)
        {

        }
    }
}