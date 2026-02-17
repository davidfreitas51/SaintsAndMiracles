using Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.Services;

public class CacheServiceTests
{
    private CacheService CreateService()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        return new CacheService(memoryCache, NullLogger<CacheService>.Instance);
    }

    [Fact]
    public void GetNextVersion_ShouldIncrementAndWrapAround()
    {
        var service = CreateService();
        string prefix = "test";

        int v1 = service.GetNextVersion(prefix);
        int v2 = service.GetNextVersion(prefix);

        Assert.Equal(1, v1);
        Assert.Equal(2, v2);

        // Force wrap around by setting version near MaxVersion
        for (int i = 0; i < 2999998; i++)
            service.GetNextVersion(prefix);

        int wrap = service.GetNextVersion(prefix);
        Assert.Equal(1, wrap); // Wrapped
    }

    [Fact]
    public void GetCurrentVersion_ShouldReturnCorrectVersion()
    {
        var service = CreateService();
        string prefix = "currentTest";

        Assert.Equal(0, service.GetCurrentVersion(prefix));
        service.GetNextVersion(prefix);
        Assert.Equal(1, service.GetCurrentVersion(prefix));
    }

    [Fact]
    public void BuildKey_ShouldIncludeVersion()
    {
        var service = CreateService();
        string key = service.BuildKey("main", "part", incrementVersion: true);

        Assert.StartsWith("main_part_v", key);
        Assert.Contains("_v", key);

        string keyNoInc = service.BuildKey("main", "part", incrementVersion: false);
        Assert.StartsWith("main_part_v", keyNoInc);
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldCacheReferenceType()
    {
        var service = CreateService();
        string key = "refKey";

        int fetchCount = 0;
        Func<Task<string?>> fetch = async () =>
        {
            fetchCount++;
            await Task.Delay(1);
            return "value";
        };

        var first = await service.GetOrSetAsync(key, fetch);
        var second = await service.GetOrSetAsync(key, fetch);

        Assert.Equal("value", first);
        Assert.Equal("value", second);
        Assert.Equal(1, fetchCount); // Cached
    }

    [Fact]
    public async Task GetOrSetValueAsync_ShouldCacheValueType()
    {
        var service = CreateService();
        string key = "valKey";

        int fetchCount = 0;
        Func<Task<int>> fetch = async () =>
        {
            fetchCount++;
            await Task.Delay(1);
            return 42;
        };

        int first = await service.GetOrSetValueAsync(key, fetch);
        int second = await service.GetOrSetValueAsync(key, fetch);

        Assert.Equal(42, first);
        Assert.Equal(42, second);
        Assert.Equal(1, fetchCount); // Cached
    }

    [Fact]
    public async Task Remove_ShouldEvictCache()
    {
        var service = CreateService();
        string key = "toRemove";

        await service.GetOrSetAsync(key, () => Task.FromResult("cached" as string));
        service.Remove(key);

        // Should fetch again after removal
        int fetchCount = 0;
        Func<Task<string?>> fetch = () =>
        {
            fetchCount++;
            return Task.FromResult("newValue" as string);
        };

        var value = await service.GetOrSetAsync(key, fetch);
        Assert.Equal("newValue", value);
        Assert.Equal(1, fetchCount);
    }
}
