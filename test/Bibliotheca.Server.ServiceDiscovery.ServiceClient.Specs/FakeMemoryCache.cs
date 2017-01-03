using Microsoft.Extensions.Caching.Memory;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Specs
{
    public class FakeMemoryCache : IMemoryCache
    {
        public ICacheEntry CreateEntry(object key)
        {
            return new CacheEntry();
        }

        public void Dispose()
        {
        }

        public void Remove(object key)
        {
        }

        public bool TryGetValue(object key, out object value)
        {
            value = null;
            return false;
        }
    }
}