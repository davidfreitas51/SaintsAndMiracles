using Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.Services;

public class CacheServiceTests
{
    private static CacheService CreateService()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        return new CacheService(memoryCache, NullLogger<CacheService>.Instance);
    }

    private static async Task<string?> FetchStringAsync(string value, Action onFetch)
    {
        onFetch();
        await Task.Delay(1);
        return value;
    }

    private static async Task<int> FetchIntAsync(int value, Action onFetch)
    {
        onFetch();
        await Task.Delay(1);
        return value;
    }

    [Fact]
    public void GetNextVersion_ShouldIncrementAndWrapAround()
    {
        var service = CreateService();
        const string prefix = "test";

        var firstVersion = service.GetNextVersion(prefix);
        var secondVersion = service.GetNextVersion(prefix);

        Assert.Equal(1, firstVersion);
        Assert.Equal(2, secondVersion);

        for (int i = 0; i < 2999998; i++)
        {
            service.GetNextVersion(prefix);
        }

        var wrappedVersion = service.GetNextVersion(prefix);
        Assert.Equal(1, wrappedVersion);
    }

    [Fact]
    public void GetCurrentVersion_ShouldReturnCorrectVersion()
    {
        var service = CreateService();
        const string prefix = "currentTest";

        Assert.Equal(0, service.GetCurrentVersion(prefix));
        service.GetNextVersion(prefix);
        Assert.Equal(1, service.GetCurrentVersion(prefix));
    }

    [Fact]
    public void BuildKey_ShouldIncludeVersion()
    {
        var service = CreateService();
        var key = service.BuildKey("main", "part", incrementVersion: true);

        Assert.StartsWith("main_part_v", key);
        Assert.Contains("_v", key);

        var keyWithoutIncrement = service.BuildKey("main", "part", incrementVersion: false);
        Assert.StartsWith("main_part_v", keyWithoutIncrement);
    }

    [Fact]
    public void BuildKey_ShouldUseCurrentVersion_WhenIncrementVersionIsFalse()
    {
        var service = CreateService();

        var key = service.BuildKey("noinc", "part", incrementVersion: false);

        Assert.Equal("noinc_part_v0", key);
        Assert.Equal(0, service.GetCurrentVersion("noinc"));
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldCacheReferenceType()
    {
        var service = CreateService();
        const string key = "refKey";

        var fetchCount = 0;
        Task<string?> Fetch() => FetchStringAsync("value", () => fetchCount++);

        var first = await service.GetOrSetAsync(key, Fetch);
        var second = await service.GetOrSetAsync(key, Fetch);

        Assert.Equal("value", first);
        Assert.Equal("value", second);
        Assert.Equal(1, fetchCount);
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldNotCacheNullValue()
    {
        var service = CreateService();
        const string key = "nullRefKey";

        var fetchCount = 0;
        async Task<string?> Fetch()
        {
            fetchCount++;
            await Task.Delay(1);
            return null;
        }

        var first = await service.GetOrSetAsync(key, Fetch);
        var second = await service.GetOrSetAsync(key, Fetch);

        Assert.Null(first);
        Assert.Null(second);
        Assert.Equal(2, fetchCount);
    }

    [Fact]
    public async Task GetOrSetValueAsync_ShouldCacheValueType()
    {
        var service = CreateService();
        const string key = "valKey";

        var fetchCount = 0;
        Task<int> Fetch() => FetchIntAsync(42, () => fetchCount++);

        var first = await service.GetOrSetValueAsync(key, Fetch);
        var second = await service.GetOrSetValueAsync(key, Fetch);

        Assert.Equal(42, first);
        Assert.Equal(42, second);
        Assert.Equal(1, fetchCount);
    }

    [Fact]
    public async Task Remove_ShouldEvictCache()
    {
        var service = CreateService();
        const string key = "toRemove";

        await service.GetOrSetAsync(key, () => Task.FromResult<string?>("cached"));
        service.Remove(key);

        var fetchCount = 0;
        Task<string?> Fetch()
        {
            fetchCount++;
            return Task.FromResult<string?>("newValue");
        }

        var value = await service.GetOrSetAsync(key, Fetch);
        Assert.Equal("newValue", value);
        Assert.Equal(1, fetchCount);
    }
}
