using Core.Interfaces.Services;

namespace Common;

public class DummyCacheService : ICacheService
{
    private readonly Dictionary<string, object?> _cache = new();
    private readonly Dictionary<string, int> _versions = new();

    public string BuildKey(string prefix, string identifier, bool incrementVersion = true) =>
        $"{prefix}:{identifier}";

    Task<T?> ICacheService.GetOrSetAsync<T>(string cacheKey, Func<Task<T?>> fetchFromDb) where T : class
        => _cache.TryGetValue(cacheKey, out var cached) ? Task.FromResult(cached as T) : fetchFromDb();

    Task<T> ICacheService.GetOrSetValueAsync<T>(string key, Func<Task<T>> fetch) where T : struct
        => _cache.TryGetValue(key, out var cached) ? Task.FromResult((T)cached!) : fetch();

    public void Remove(string key) => _cache.Remove(key);

    public int GetCurrentVersion(string prefix)
    {
        _versions.TryGetValue(prefix, out var version);
        return version;
    }

    public int GetNextVersion(string prefix)
    {
        _versions.TryGetValue(prefix, out var version);
        version++;
        _versions[prefix] = version;
        return version;
    }
}
