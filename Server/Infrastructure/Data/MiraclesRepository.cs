using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.Filters;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class MiraclesRepository(DataContext context, ICacheService cacheService) : IMiraclesRepository
{
    public async Task<PagedResult<Miracle>> GetAllAsync(MiracleFilters filters)
    {
        var tagPart = filters.TagIds != null && filters.TagIds.Count > 0
            ? string.Join("-", filters.TagIds)
            : string.Empty;

        var cacheKey = cacheService.BuildKey(
            "miracle",
            $"list_page{filters.PageNumber}_size{filters.PageSize}_search{filters.Search}_country{filters.Country}_century{filters.Century}_tags{tagPart}_order{filters.OrderBy}",
            incrementVersion: false
        );

        var result = await cacheService.GetOrSetAsync(
            cacheKey,
            async () => await FetchMiraclesFromDb(filters)
        )!;

        return result ?? new PagedResult<Miracle>();
    }

    public async Task<Miracle?> GetByIdAsync(int id)
    {
        var cacheKey = cacheService.BuildKey("miracle", $"id{id}", incrementVersion: false);
        return await cacheService.GetOrSetAsync(
            cacheKey,
            () => context.Miracles.Include(m => m.Tags).FirstOrDefaultAsync(m => m.Id == id)
        );
    }

    public async Task<Miracle?> GetBySlugAsync(string slug)
    {
        var cacheKey = cacheService.BuildKey("miracle", $"slug{slug}", incrementVersion: false);
        return await cacheService.GetOrSetAsync(
            cacheKey,
            () => context.Miracles.Include(m => m.Tags).FirstOrDefaultAsync(m => m.Slug == slug)
        );
    }

    public async Task<bool> CreateAsync(Miracle newMiracle)
    {
        newMiracle.CreatedAt = DateTime.UtcNow;
        newMiracle.UpdatedAt = DateTime.UtcNow;

        await context.Miracles.AddAsync(newMiracle);
        var created = await context.SaveChangesAsync() > 0;

        if (created)
            InvalidateMiracleCaches(newMiracle);

        return created;
    }

    public async Task<bool> UpdateAsync(Miracle miracle)
    {
        var trackedMiracle = await context.Miracles
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == miracle.Id);

        if (trackedMiracle == null)
            return false;

        trackedMiracle.Title = miracle.Title;
        trackedMiracle.Description = miracle.Description;
        trackedMiracle.Country = miracle.Country;
        trackedMiracle.Century = miracle.Century;
        trackedMiracle.Date = miracle.Date;
        trackedMiracle.UpdatedAt = DateTime.UtcNow;

        trackedMiracle.Tags.RemoveAll(t => !miracle.Tags.Any(mt => mt.Id == t.Id));

        foreach (var tag in miracle.Tags)
        {
            if (!trackedMiracle.Tags.Any(t => t.Id == tag.Id))
            {
                var existingTag = await context.Tags.FindAsync(tag.Id) ?? tag;
                trackedMiracle.Tags.Add(existingTag);
            }
        }

        var updated = await context.SaveChangesAsync() > 0;

        if (updated)
            InvalidateMiracleCaches(trackedMiracle);

        return updated;
    }

    public async Task DeleteAsync(int id)
    {
        var miracle = await context.Miracles
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (miracle is not null)
        {
            context.Miracles.Remove(miracle);
            var deleted = await context.SaveChangesAsync() > 0;

            if (deleted)
                InvalidateMiracleCaches(miracle);
        }
    }

    public async Task<IReadOnlyList<string>> GetCountriesAsync()
    {
        var cacheKey = cacheService.BuildKey("miracle", "countries_all", incrementVersion: false);

        var countries = await cacheService.GetOrSetAsync(
            cacheKey,
            async () => await context.Miracles
                .Select(m => m.Country)
                .Where(c => c != null && c != "")
                .Distinct()
                .ToListAsync()
        );

        return countries ?? [];
    }

    public async Task<int> GetTotalMiraclesAsync()
    {
        var cacheKey = cacheService.BuildKey("miracle", "total_count", incrementVersion: false);
        return await cacheService.GetOrSetValueAsync(
            cacheKey,
            () => context.Miracles.CountAsync()
        );
    }

    public async Task<bool> SlugExistsAsync(string slug)
    {
        var cacheKey = cacheService.BuildKey("miracle", $"slugexists_{slug}", incrementVersion: false);
        return await cacheService.GetOrSetValueAsync(
            cacheKey,
            () => context.Miracles.AnyAsync(m => m.Slug == slug)
        );
    }

    private async Task<PagedResult<Miracle>> FetchMiraclesFromDb(MiracleFilters filters)
    {
        var query = context.Miracles.Include(m => m.Tags).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.Country))
            query = query.Where(m => m.Country == filters.Country);

        if (!string.IsNullOrWhiteSpace(filters.Century))
            query = query.Where(m => m.Century.ToString() == filters.Century);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.ToLower();
            query = query.Where(m =>
                EF.Functions.Like(m.Title.ToLower(), $"%{search}%") ||
                EF.Functions.Like(m.Description.ToLower(), $"%{search}%"));
        }

        if (filters.TagIds is { Count: > 0 })
            query = query.Where(m => m.Tags.Any(tag => filters.TagIds.Contains(tag.Id)));

        query = filters.OrderBy switch
        {
            MiracleOrderBy.Title => query.OrderBy(m => m.Title),
            MiracleOrderBy.TitleDesc => query.OrderByDescending(m => m.Title),
            MiracleOrderBy.Century => query.OrderBy(m => m.Century),
            MiracleOrderBy.CenturyDesc => query.OrderByDescending(m => m.Century),
            _ => query.OrderBy(m => m.Title)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return new PagedResult<Miracle>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    private void InvalidateMiracleCaches(Miracle miracle)
    {
        cacheService.Remove(cacheService.BuildKey("miracle", $"id{miracle.Id}", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("miracle", $"slug{miracle.Slug}", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("miracle", "list_all", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("miracle", "total_count", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("miracle", "countries_all", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("miracle", $"slugexists_{miracle.Slug}", incrementVersion: false));

        cacheService.GetNextVersion("miracle");
    }
}
