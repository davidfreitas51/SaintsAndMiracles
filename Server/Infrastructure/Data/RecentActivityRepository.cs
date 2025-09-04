using Core.Interfaces.Repositories;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class RecentActivityRepository(DataContext context) : IRecentActivityRepository
{
    private const int MaxRecords = 100;

    public async Task<List<RecentActivity>> GetRecentActivitiesAsync(int limit = 5)
    {
        return await context.RecentActivities
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }
    public async Task LogActivityAsync(string entityName, int entityId, string displayName, string action, string? userId = null)
    {
        var activity = new RecentActivity
        {
            EntityName = entityName,
            EntityId = entityId,
            DisplayName = displayName,
            Action = action,
            UserId = userId
        };

        context.RecentActivities.Add(activity);
        await context.SaveChangesAsync();

        int totalRecords = await context.RecentActivities.CountAsync();
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
    }
}
