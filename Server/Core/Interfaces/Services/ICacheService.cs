namespace Core.Interfaces.Services;

public interface ICacheService
{
    Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T?>> fetchFromDb) where T : class;
    Task<T> GetOrSetValueAsync<T>(string key, Func<Task<T>> fetch) where T : struct;
    void Remove(string cacheKey);
    int GetCurrentVersion(string prefix);
    int GetNextVersion(string prefix);
    string BuildKey(string prefix, string identifier, bool incrementVersion = true);
}
