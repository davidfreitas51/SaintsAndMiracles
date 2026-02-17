using Common;
using Core.Enums;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.Data;

public class RecentActivityRepositoryTests
{
    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_ShouldReturnPagedResults()
    {
        using var context = CreateContext();
        var cacheService = new DummyCacheService();
        var repo = new RecentActivityRepository(context, cacheService, NullLogger<RecentActivityRepository>.Instance);

        for (int i = 1; i <= 10; i++)
        {
            context.RecentActivities.Add(new RecentActivity
            {
                EntityName = "Prayer",
                EntityId = i,
                DisplayName = $"Prayer {i}",
                Action = "created",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();

        var result = await repo.GetRecentActivitiesAsync(pageNumber: 2, pageSize: 3);

        Assert.NotNull(result);
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(3, result.Items.Count());
        Assert.True(result.Items.First().CreatedAt > result.Items.Last().CreatedAt);
    }

    [Fact]
    public async Task LogActivityAsync_ShouldInsertNewRecord()
    {
        using var context = CreateContext();
        var cacheService = new DummyCacheService();
        var repo = new RecentActivityRepository(context, cacheService, NullLogger<RecentActivityRepository>.Instance);

        await repo.LogActivityAsync(EntityType.Prayer, 1, "Our Father", ActivityAction.Created, "user123");

        var activity = await context.RecentActivities.FirstOrDefaultAsync();
        Assert.NotNull(activity);
        Assert.Equal("Prayer", activity.EntityName);
        Assert.Equal(1, activity.EntityId);
        Assert.Equal("Our Father", activity.DisplayName);
        Assert.Equal("created", activity.Action);
        Assert.Equal("user123", activity.UserId);
    }

    [Fact]
    public async Task LogActivityAsync_ShouldTrimOldRecords_WhenOverLimit()
    {
        using var context = CreateContext();
        var cacheService = new DummyCacheService();
        var repo = new RecentActivityRepository(context, cacheService, NullLogger<RecentActivityRepository>.Instance);

        // Cria 101 registros antigos
        for (int i = 0; i < 101; i++)
        {
            context.RecentActivities.Add(new RecentActivity
            {
                EntityName = "Miracle",
                EntityId = i,
                DisplayName = $"Miracle {i}",
                Action = "created",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();

        await repo.LogActivityAsync(EntityType.Miracle, 999, "New Miracle", ActivityAction.Created);

        var total = await context.RecentActivities.CountAsync();
        Assert.Equal(100, total); // Deve manter o limite
    }

    [Fact]
    public async Task LogActivityAsync_ShouldIncrementCacheVersion()
    {
        using var context = CreateContext();
        var cacheService = new DummyCacheService();
        var repo = new RecentActivityRepository(context, cacheService, NullLogger<RecentActivityRepository>.Instance);

        int before = cacheService.GetNextVersion("recent_activity");
        await repo.LogActivityAsync(EntityType.Saint, 1, "St. Francis", ActivityAction.Updated);
        int after = cacheService.GetNextVersion("recent_activity");

        Assert.True(after > before);
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_ShouldReturnEmpty_WhenNoRecords()
    {
        using var context = CreateContext();
        var cacheService = new DummyCacheService();
        var repo = new RecentActivityRepository(context, cacheService, NullLogger<RecentActivityRepository>.Instance);

        var result = await repo.GetRecentActivitiesAsync();

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
