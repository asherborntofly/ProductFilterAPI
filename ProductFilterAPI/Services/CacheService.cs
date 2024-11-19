using Microsoft.Extensions.Caching.Memory;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultExpirationTime = TimeSpan.FromMinutes(5);

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T? GetFromCache<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return value;
    }

    public void SetCache<T>(string key, T value, TimeSpan? expirationTime = null)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expirationTime ?? _defaultExpirationTime
        };

        _cache.Set(key, value, cacheOptions);
    }
} 