using System.Collections.Concurrent;
using Core.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services;

public class CacheService(IMemoryCache cache) : ICacheService
{
    private readonly TimeSpan _slidingExpiration = TimeSpan.FromMinutes(60);
    private static readonly ConcurrentDictionary<string, int> Versions = new ConcurrentDictionary<string, int>();
    private const int MaxVersion = 3000000;

    public int GetNextVersion(string mainPrefix)
    {
        return Versions.AddOrUpdate(
            mainPrefix,
            1,
            (_, current) => (current + 1) > MaxVersion ? 1 : current + 1
        );
    }

    public int GetCurrentVersion(string mainPrefix)
    {
        return Versions.TryGetValue(mainPrefix, out int current) ? current : 0;
    }

    public string BuildKey(string mainPrefix, string keyPart, bool incrementVersion = true)
    {
        int version = incrementVersion ? GetNextVersion(mainPrefix) : GetCurrentVersion(mainPrefix);
        return $"{mainPrefix}_{keyPart}_v{version}";
    }

    public async Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T?>> fetchFromDb) where T : class
    {
        if (cache.TryGetValue(cacheKey, out T? cached))
            return cached;

        var result = await fetchFromDb();

        if (result is not null)
        {
            await Task.Delay(3000);
            cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _slidingExpiration
            });
        }

        return result;
    }

    public async Task<T> GetOrSetValueAsync<T>(string key, Func<Task<T>> fetch) where T : struct
    {
        if (cache.TryGetValue(key, out T cached))
            return cached;

        var result = await fetch();

        cache.Set(key, result, new MemoryCacheEntryOptions
        {
            SlidingExpiration = _slidingExpiration
        });

        return result;
    }

    public void Remove(string cacheKey)
    {
        cache.Remove(cacheKey);
    }
}
