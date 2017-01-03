using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Bibliotheca.Server.ServiceDiscovery.ServiceClient.Specs
{
    public class CacheEntry : ICacheEntry
    {
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        public IList<IChangeToken> ExpirationTokens
        {
            get
            {
                return new List<IChangeToken>();
            }
        }

        public object Key
        {
            get
            {
                return string.Empty;
            }
        }

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks
        {
            get
            {
                return new List<PostEvictionCallbackRegistration>();
            }
        }

        public CacheItemPriority Priority { get; set; }

        public TimeSpan? SlidingExpiration{ get; set; }

        public object Value { get; set; }

        public void Dispose()
        {
        }
    }
}