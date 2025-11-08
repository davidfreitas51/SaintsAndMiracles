using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class RecentActivityRepository(DataContext context, ICacheService cacheService) : IRecentActivityRepository
{
    private const int MaxRecords = 100;

    public async Task<PagedResult<RecentActivity>> GetRecentActivitiesAsync(int pageNumber = 1, int pageSize = 5)
    {
        var cacheKey = cacheService.BuildKey(
            "recent_activity",
            $"page{pageNumber}_size{pageSize}",
            incrementVersion: false
        );

        var result = await cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var totalRecords = await context.RecentActivities.CountAsync();

                var items = await context.RecentActivities
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<RecentActivity>
                {
                    Items = items,
                    TotalCount = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        );

        return result ?? new PagedResult<RecentActivity>();
    }

    public async Task LogActivityAsync(EntityType entityType, int entityId, string displayName, ActivityAction action, string? userId = null)
    {
        var activity = new RecentActivity
        {
            EntityName = entityType.ToString(),
            EntityId = entityId,
            DisplayName = displayName,
            Action = action.ToString().ToLower(),
            UserId = userId
        };

        context.RecentActivities.Add(activity);
        await context.SaveChangesAsync();

        var totalRecords = await context.RecentActivities.CountAsync();
        if (totalRecords > MaxRecords)
        {
            int excess = totalRecords - MaxRecords;
            var oldest = await context.RecentActivities
                .OrderBy(a => a.CreatedAt)
                .Take(excess)
                .ToListAsync();

            context.RecentActivities.RemoveRange(oldest);
            await context.SaveChangesAsync();
        }

        cacheService.GetNextVersion("recent_activity");
    }
}
