using Document.Api.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Document.Api.Infrastructure.Persistance
{
    public class CacheService(IMemoryCache cache) : ICacheService
    {
        private readonly IMemoryCache _cache = cache;
        private readonly List<string> _keys = new List<string>();

        public void SetCache(string key, object value)
        {
            _cache.Set(key, value, TimeSpan.FromMinutes(5));
            _keys.Add(key);
        }

        public bool TryGetCache(string key, out object value)
        {
            return _cache.TryGetValue(key, out value!); 
        }

        public void InvalidateCaches()
        {
            foreach (var key in _keys)
            {
                _cache.Remove(key);
            }

            _keys.Clear();
        }
    }
}
