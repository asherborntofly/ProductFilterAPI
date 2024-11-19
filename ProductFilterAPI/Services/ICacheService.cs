public interface ICacheService
{
    T? GetFromCache<T>(string key);
    void SetCache<T>(string key, T value, TimeSpan? expirationTime = null);
} 