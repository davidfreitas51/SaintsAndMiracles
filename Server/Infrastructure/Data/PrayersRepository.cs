using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.Filters;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class PrayersRepository(DataContext context, ICacheService cacheService) : IPrayersRepository
{
    public async Task<PagedResult<Prayer>> GetAllAsync(PrayerFilters filters)
    {
        var tagPart = filters.TagIds != null && filters.TagIds.Count > 0
            ? string.Join("-", filters.TagIds)
            : string.Empty;

        var cacheKey = cacheService.BuildKey(
            "prayer",
            $"list_page{filters.PageNumber}_size{filters.PageSize}_search{filters.Search}_tags{tagPart}_order{filters.OrderBy}",
            incrementVersion: false
        );

        var result = await cacheService.GetOrSetAsync(
            cacheKey,
            async () => await FetchPrayersFromDb(filters)
        )!;

        return result ?? new PagedResult<Prayer>();
    }

    public async Task<Prayer?> GetByIdAsync(int id)
    {
        var cacheKey = cacheService.BuildKey("prayer", $"id{id}", incrementVersion: false);
        return await cacheService.GetOrSetAsync(
            cacheKey,
            () => context.Prayers.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id)
        );
    }

    public async Task<Prayer?> GetBySlugAsync(string slug)
    {
        var cacheKey = cacheService.BuildKey("prayer", $"slug{slug}", incrementVersion: false);
        return await cacheService.GetOrSetAsync(
            cacheKey,
            () => context.Prayers.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Slug == slug)
        );
    }

    public async Task<bool> CreateAsync(Prayer newPrayer)
    {
        newPrayer.CreatedAt = DateTime.UtcNow;
        newPrayer.UpdatedAt = DateTime.UtcNow;

        await context.Prayers.AddAsync(newPrayer);
        var created = await context.SaveChangesAsync() > 0;

        if (created)
            InvalidatePrayerCaches(newPrayer);

        return created;
    }

    public async Task<bool> UpdateAsync(Prayer prayer)
    {
        var trackedPrayer = await context.Prayers
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == prayer.Id);

        if (trackedPrayer == null)
            return false;

        trackedPrayer.Title = prayer.Title;
        trackedPrayer.Description = prayer.Description;
        trackedPrayer.Slug = prayer.Slug;
        trackedPrayer.MarkdownPath = prayer.MarkdownPath;
        trackedPrayer.Image = prayer.Image;
        trackedPrayer.Tags = prayer.Tags;
        trackedPrayer.UpdatedAt = DateTime.UtcNow;

        trackedPrayer.Tags.RemoveAll(t => !prayer.Tags.Any(pt => pt.Id == t.Id));

        foreach (var tag in prayer.Tags)
        {
            if (!trackedPrayer.Tags.Any(t => t.Id == tag.Id))
            {
                var existingTag = await context.Tags.FindAsync(tag.Id) ?? tag;
                trackedPrayer.Tags.Add(existingTag);
            }
        }

        var updated = await context.SaveChangesAsync() > 0;

        if (updated)
            InvalidatePrayerCaches(trackedPrayer);

        return updated;
    }

    public async Task<bool> DeleteAsync(Prayer prayer)
    {
        context.Prayers.Remove(prayer);
        var deleted = await context.SaveChangesAsync() > 0;

        if (deleted)
            InvalidatePrayerCaches(prayer);

        return deleted;
    }

    public async Task<bool> SlugExistsAsync(string slug)
    {
        var cacheKey = cacheService.BuildKey("prayer", $"slugexists_{slug}", incrementVersion: false);
        return await cacheService.GetOrSetValueAsync(
            cacheKey,
            () => context.Prayers.AnyAsync(p => p.Slug == slug)
        );
    }

    public async Task<IReadOnlyList<string>> GetTagsAsync()
    {
        var cacheKey = cacheService.BuildKey("prayer", "tags_all", incrementVersion: false);

        var tags = await cacheService.GetOrSetAsync(
            cacheKey,
            async () => await context.Prayers
                .SelectMany(p => p.Tags)
                .Select(t => t.Name)
                .Distinct()
                .ToListAsync()
        );

        return tags ?? [];
    }

    public async Task<int> GetTotalPrayersAsync()
    {
        var cacheKey = cacheService.BuildKey("prayer", "total_count", incrementVersion: false);
        return await cacheService.GetOrSetValueAsync(
            cacheKey,
            () => context.Prayers.CountAsync()
        );
    }

    private async Task<PagedResult<Prayer>> FetchPrayersFromDb(PrayerFilters filters)
    {
        var query = context.Prayers.Include(p => p.Tags).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.ToLower();
            query = query.Where(p =>
                EF.Functions.Like(p.Title.ToLower(), $"%{search}%") ||
                EF.Functions.Like(p.Description.ToLower(), $"%{search}%"));
        }

        if (filters.TagIds is { Count: > 0 })
            query = query.Where(p => p.Tags.Any(tag => filters.TagIds.Contains(tag.Id)));

        query = filters.OrderBy switch
        {
            PrayerOrderBy.Title => query.OrderBy(p => p.Title),
            PrayerOrderBy.TitleDesc => query.OrderByDescending(p => p.Title),
            _ => query.OrderBy(p => p.Title)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return new PagedResult<Prayer>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    private void InvalidatePrayerCaches(Prayer prayer)
    {
        cacheService.Remove(cacheService.BuildKey("prayer", $"id{prayer.Id}", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("prayer", $"slug{prayer.Slug}", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("prayer", "list_all", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("prayer", "total_count", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("prayer", "tags_all", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("prayer", $"slugexists_{prayer.Slug}", incrementVersion: false));

        cacheService.GetNextVersion("prayer");
    }
}
