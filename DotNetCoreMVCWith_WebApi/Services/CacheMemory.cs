using Microsoft.Extensions.Caching.Memory;

namespace DotNetCoreMVCWith_WebApi.Services
{
    public class CacheMemory
    {
        private readonly IMemoryCache _cache;
        public CacheMemory(IMemoryCache cache)
        {
            _cache=cache;
        }

        public void setcache<T>(string key,T value,int Minutes)
        {
            var expire= new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow=TimeSpan.FromMinutes(Minutes) };
            _cache.Set(key, value, expire);
        }
        public List<T> getcache<T>(string key = "")
        {
           return (List<T>)_cache.Get(key);
        }
        public void Removecache(string key = "")
        {
             _cache.Remove(key);
        }
    }
}
