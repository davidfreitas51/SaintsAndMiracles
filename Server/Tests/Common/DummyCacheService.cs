namespace Tests.Common;

using System.Collections.Concurrent;
using Core.Interfaces.Services;

public class DummyCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, object?> _cache = new();
    private readonly ConcurrentDictionary<string, int> _versions = new();

    public string BuildKey(string prefix, string identifier, bool incrementVersion = true)
        => $"{prefix}:{identifier}";

    Task<T?> ICacheService.GetOrSetAsync<T>(string key, Func<Task<T?>> fetchFromDb) where T : class
        => _cache.TryGetValue(key, out var cached) ? Task.FromResult(cached as T) : fetchFromDb();

    Task<T> ICacheService.GetOrSetValueAsync<T>(string key, Func<Task<T>> fetch) where T : struct
        => _cache.TryGetValue(key, out var cached) ? Task.FromResult((T)cached!) : fetch();

    public void Remove(string key) => _cache.TryRemove(key, out _);

    public int GetCurrentVersion(string prefix)
        => _versions.TryGetValue(prefix, out var version) ? version : 0;

    public int GetNextVersion(string prefix)
    {
        var version = _versions.AddOrUpdate(prefix, 1, (_, old) => old + 1);
        return version;
    }
}
