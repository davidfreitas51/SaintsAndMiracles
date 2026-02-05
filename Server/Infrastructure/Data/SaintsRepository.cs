using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.Filters;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class SaintsRepository(DataContext context, ICacheService cacheService) : ISaintsRepository
{
    public async Task<PagedResult<Saint>> GetAllAsync(SaintFilters filters)
    {
        var tagPart = filters.TagIds != null && filters.TagIds.Count > 0
            ? string.Join("-", filters.TagIds)
            : string.Empty;

        var cacheKey = cacheService.BuildKey(
            "saint",
            $"list_page{filters.PageNumber}_size{filters.PageSize}_search{filters.Search}_country{filters.Country}_century{filters.Century}_feastmonth{filters.FeastMonth}_order{filters.OrderBy}_orderid{filters.ReligiousOrderId}_tags{tagPart}",
            incrementVersion: false
        );

        var result = await cacheService.GetOrSetAsync(
            cacheKey,
            async () => await FetchSaintsFromDb(filters)
        )!;

        return result ?? new PagedResult<Saint>();
    }

    public async Task<Saint?> GetByIdAsync(int id)
    {
        var cacheKey = cacheService.BuildKey("saint", $"id{id}", incrementVersion: false);
        return await cacheService.GetOrSetAsync(
            cacheKey,
            () => context.Saints
                .Include(s => s.Tags)
                .Include(s => s.ReligiousOrder)
                .FirstOrDefaultAsync(s => s.Id == id)
        );
    }

    public async Task<Saint?> GetBySlugAsync(string slug)
    {
        var cacheKey = cacheService.BuildKey("saint", $"slug{slug}", incrementVersion: false);
        return await cacheService.GetOrSetAsync(
            cacheKey,
            () => context.Saints
                .Include(s => s.Tags)
                .Include(s => s.ReligiousOrder)
                .FirstOrDefaultAsync(s => s.Slug == slug)
        );
    }

    public async Task<bool> CreateAsync(Saint newSaint)
    {
        newSaint.CreatedAt = DateTime.UtcNow;
        newSaint.UpdatedAt = DateTime.UtcNow;

        await context.Saints.AddAsync(newSaint);
        var created = await context.SaveChangesAsync() > 0;

        if (created)
            InvalidateSaintCaches(newSaint);

        return created;
    }

    public async Task<bool> UpdateAsync(Saint saint)
    {
        var trackedSaint = await context.Saints
            .Include(s => s.Tags)
            .Include(s => s.ReligiousOrder)
            .FirstOrDefaultAsync(s => s.Id == saint.Id);

        if (trackedSaint == null)
            return false;

        trackedSaint.Name = saint.Name;
        trackedSaint.Description = saint.Description;
        trackedSaint.Country = saint.Country;
        trackedSaint.Century = saint.Century;
        trackedSaint.FeastDay = saint.FeastDay;
        trackedSaint.ReligiousOrderId = saint.ReligiousOrderId;
        trackedSaint.UpdatedAt = DateTime.UtcNow;

        trackedSaint.Tags.RemoveAll(t => !saint.Tags.Any(st => st.Id == t.Id));

        foreach (var tag in saint.Tags)
        {
            if (!trackedSaint.Tags.Any(t => t.Id == tag.Id))
            {
                var existingTag = await context.Tags.FindAsync(tag.Id) ?? tag;
                trackedSaint.Tags.Add(existingTag);
            }
        }

        var updated = await context.SaveChangesAsync() > 0;

        if (updated)
            InvalidateSaintCaches(trackedSaint);

        return updated;
    }

    public async Task DeleteAsync(int id)
    {
        var saint = await context.Saints
            .Include(s => s.Tags)
            .Include(s => s.ReligiousOrder)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (saint is null)
            return;

        context.Saints.Remove(saint);
        var deleted = await context.SaveChangesAsync() > 0;

        if (deleted)
            InvalidateSaintCaches(saint);
    }

    public async Task<IReadOnlyList<string>> GetCountriesAsync()
    {
        var cacheKey = cacheService.BuildKey("saint", "countries_all", incrementVersion: false);

        var countries = await cacheService.GetOrSetAsync(
            cacheKey,
            async () => await context.Saints
                .Select(s => s.Country)
                .Where(c => c != null && c != "")
                .Distinct()
                .ToListAsync()
        );

        return countries ?? [];
    }

    public async Task<int> GetTotalSaintsAsync()
    {
        var cacheKey = cacheService.BuildKey("saint", "total_count", incrementVersion: false);
        return await cacheService.GetOrSetValueAsync(
            cacheKey,
            () => context.Saints.CountAsync()
        );
    }

    public async Task<bool> SlugExistsAsync(string slug)
    {
        var cacheKey = cacheService.BuildKey("saint", $"slugexists_{slug}", incrementVersion: false);
        return await cacheService.GetOrSetValueAsync(
            cacheKey,
            () => context.Saints.AnyAsync(s => s.Slug == slug)
        );
    }

    public async Task<List<Saint>> GetSaintsOfTheDayAsync(DateOnly feastDay)
    {
        var cacheKey = cacheService.BuildKey(
            "saint",
            $"oftheday_{feastDay:MMdd}",
            incrementVersion: false
        );

        var cachedResult = await cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var saints = await context.Saints
                    .Include(s => s.Tags)
                    .Include(s => s.ReligiousOrder)
                    .Where(s =>
                        s.FeastDay.HasValue &&
                        s.FeastDay.Value.Month == feastDay.Month &&
                        s.FeastDay.Value.Day == feastDay.Day &&
                        s.Tags.Any(t => t.Name == "Saint of the Day")
                    )
                    .ToListAsync();

                return saints;
            });

        return cachedResult ?? new List<Saint>();
    }

    public async Task<List<Saint>> GetUpcomingFeasts(DateOnly today, int take = 10)
    {
        var cacheKey = cacheService.BuildKey(
            "saint",
            $"upcomingfeasts_{today:MMDD}",
            incrementVersion: false
        );

        var cachedResult = await cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var saints = await context.Saints
                    .Include(s => s.Tags)
                    .Include(s => s.ReligiousOrder)
                    .Where(s => s.FeastDay.HasValue)
                    .ToListAsync();

                var upcoming = saints
                    .Select(s => new
                    {
                        Saint = s,
                        NextFeast = GetNextFeastDate(s.FeastDay!.Value, today)
                    })
                    .Where(x => x.NextFeast > today)
                    .OrderBy(x => x.NextFeast)
                    .Take(take)
                    .Select(x => x.Saint)
                    .ToList();

                return upcoming;
            });

        return cachedResult ?? new List<Saint>();
    }

    private static DateOnly GetNextFeastDate(DateOnly feastDay, DateOnly today)
    {
        var thisYear = new DateOnly(today.Year, feastDay.Month, feastDay.Day);

        return thisYear >= today
            ? thisYear
            : thisYear.AddYears(1);
    }

    private async Task<PagedResult<Saint>> FetchSaintsFromDb(SaintFilters filters)
    {
        var query = context.Saints
            .Include(s => s.Tags)
            .Include(s => s.ReligiousOrder)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.Country))
            query = query.Where(s => s.Country == filters.Country);

        if (!string.IsNullOrWhiteSpace(filters.Century))
            query = query.Where(s => s.Century.ToString() == filters.Century);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.ToLower();
            query = query.Where(s =>
                EF.Functions.Like(s.Name.ToLower(), $"%{search}%") ||
                EF.Functions.Like(s.Description.ToLower(), $"%{search}%"));
        }

        if (!string.IsNullOrWhiteSpace(filters.FeastMonth) &&
            int.TryParse(filters.FeastMonth, out var month))
        {
            query = query.Where(s => s.FeastDay.HasValue && s.FeastDay.Value.Month == month);
        }

        if (!string.IsNullOrWhiteSpace(filters.ReligiousOrderId) &&
            int.TryParse(filters.ReligiousOrderId, out var orderId))
        {
            query = query.Where(s => s.ReligiousOrderId == orderId);
        }

        if (filters.TagIds is { Count: > 0 })
            query = query.Where(s => s.Tags.Any(tag => filters.TagIds.Contains(tag.Id)));

        query = filters.OrderBy switch
        {
            SaintOrderBy.Name => query.OrderBy(s => s.Name),
            SaintOrderBy.NameDesc => query.OrderByDescending(s => s.Name),
            SaintOrderBy.Century => query.OrderBy(s => s.Century),
            SaintOrderBy.CenturyDesc => query.OrderByDescending(s => s.Century),

            SaintOrderBy.FeastDay => query
                .OrderBy(s => s.FeastDay.HasValue ? s.FeastDay.Value.Month : 13)
                .ThenBy(s => s.FeastDay.HasValue ? s.FeastDay.Value.Day : 32),

            SaintOrderBy.FeastDayDesc => query
                .OrderByDescending(s => s.FeastDay.HasValue ? s.FeastDay.Value.Month : 0)
                .ThenByDescending(s => s.FeastDay.HasValue ? s.FeastDay.Value.Day : 0),


            _ => query.OrderBy(s => s.Name)
        };



        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return new PagedResult<Saint>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    private void InvalidateSaintCaches(Saint saint)
    {
        cacheService.Remove(cacheService.BuildKey("saint", $"id{saint.Id}", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("saint", $"slug{saint.Slug}", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("saint", $"slugexists_{saint.Slug}", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("saint", "list_all", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("saint", "total_count", incrementVersion: false));
        cacheService.Remove(cacheService.BuildKey("saint", "countries_all", incrementVersion: false));

        cacheService.GetNextVersion("saint");
    }
}
