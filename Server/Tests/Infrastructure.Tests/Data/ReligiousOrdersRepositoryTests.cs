using Common;
using Core.Models;
using Core.Models.Filters;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Data;

public class ReligiousOrdersRepositoryTests
{
    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }

    private static async Task SeedData(DataContext context)
    {
        var orders = new List<ReligiousOrder>
        {
            new() { Name = "Franciscan (OFM)" },
            new() { Name = "Carmelite" },
            new() { Name = "Capuchin Franciscan" }
        };

        context.ReligiousOrders.AddRange(orders);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAll_WhenNoFilter()
    {
        using var context = CreateContext();
        await SeedData(context);

        var cacheService = new DummyCacheService();
        var repo = new ReligiousOrdersRepository(context, cacheService);

        var filters = new EntityFilters { Page = 1, PageSize = 10 };
        var result = await repo.GetAllAsync(filters);

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count());
        Assert.Contains(result.Items, o => o.Name == "Franciscan (OFM)");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterBySearch()
    {
        using var context = CreateContext();
        await SeedData(context);

        var cacheService = new DummyCacheService();
        var repo = new ReligiousOrdersRepository(context, cacheService);

        var filters = new EntityFilters { Page = 1, PageSize = 10, Search = "Capuchin" };
        var result = await repo.GetAllAsync(filters);

        Assert.Single(result.Items);
        Assert.Equal("Capuchin Franciscan", result.Items.First().Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldPaginateResults()
    {
        using var context = CreateContext();
        await SeedData(context);

        var cacheService = new DummyCacheService();
        var repo = new ReligiousOrdersRepository(context, cacheService);

        var filters = new EntityFilters { Page = 2, PageSize = 2 };
        var result = await repo.GetAllAsync(filters);

        Assert.Equal(3, result.TotalCount);
        Assert.True(result.Items.Count() is 1 or 2); // Última página pode conter 1 item
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectOrder()
    {
        using var context = CreateContext();
        await SeedData(context);

        var cacheService = new DummyCacheService();
        var repo = new ReligiousOrdersRepository(context, cacheService);

        var first = await context.ReligiousOrders.FirstAsync();
        var found = await repo.GetByIdAsync(first.Id);

        Assert.NotNull(found);
        Assert.Equal(first.Name, found!.Name);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddNewOrder_AndInvalidateCache()
    {
        using var context = CreateContext();
        var cacheService = new DummyCacheService();
        var repo = new ReligiousOrdersRepository(context, cacheService);

        int beforeVersion = cacheService.GetNextVersion("religious_order");

        var order = new ReligiousOrder { Name = "Dominican" };
        var result = await repo.CreateAsync(order);

        int afterVersion = cacheService.GetNextVersion("religious_order");

        Assert.True(result);
        Assert.Single(context.ReligiousOrders);
        Assert.True(afterVersion > beforeVersion);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyOrder_AndInvalidateCache()
    {
        using var context = CreateContext();
        await SeedData(context);

        var cacheService = new DummyCacheService();
        var repo = new ReligiousOrdersRepository(context, cacheService);

        var order = await context.ReligiousOrders.FirstAsync();
        order.Name = "Franciscan Reform";

        int beforeVersion = cacheService.GetNextVersion("religious_order");
        var result = await repo.UpdateAsync(order);
        int afterVersion = cacheService.GetNextVersion("religious_order");

        var updated = await context.ReligiousOrders.FindAsync(order.Id);

        Assert.True(result);
        Assert.Equal("Franciscan Reform", updated!.Name);
        Assert.True(afterVersion > beforeVersion);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveOrder_AndInvalidateCache()
    {
        using var context = CreateContext();
        await SeedData(context);

        var cacheService = new DummyCacheService();
        var repo = new ReligiousOrdersRepository(context, cacheService);

        var order = await context.ReligiousOrders.FirstAsync();
        int beforeVersion = cacheService.GetNextVersion("religious_order");

        await repo.DeleteAsync(order.Id);
        int afterVersion = cacheService.GetNextVersion("religious_order");

        Assert.Null(await context.ReligiousOrders.FindAsync(order.Id));
        Assert.True(afterVersion > beforeVersion);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDoNothing_IfNotFound()
    {
        using var context = CreateContext();
        await SeedData(context);

        var cacheService = new DummyCacheService();
        var repo = new ReligiousOrdersRepository(context, cacheService);

        var countBefore = await context.ReligiousOrders.CountAsync();
        await repo.DeleteAsync(9999);
        var countAfter = await context.ReligiousOrders.CountAsync();

        Assert.Equal(countBefore, countAfter);
    }
}
