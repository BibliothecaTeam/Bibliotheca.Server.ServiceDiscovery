using System;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Exceptions
{
    public class ServiceDiscoveryRegistrationException : Exception
    {
        public ServiceDiscoveryRegistrationException(string message) : base(message)
        {

        }
    }
}