using Common;
using Core.Enums;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Data;

public class MiraclesRepositoryTests
{
    private MiraclesRepository CreateRepository(out DataContext context)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase("MiraclesDb")
            .Options;

        context = new DataContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var cache = new DummyCacheService();
        var repo = new MiraclesRepository(context, cache);

        context.Miracles.AddRange(GetSeedData());
        context.SaveChanges();

        return repo;
    }

    private static List<Miracle> GetSeedData() => new()
    {
        new()
        {
            Title = "The Miracle of the Sun",
            Country = "Portugal",
            Century = 20,
            Image = "img",
            Description = "desc",
            MarkdownPath = "md",
            Slug = "sun",
            Tags = new() { new Tag { Name = "Marian", TagType = TagType.Miracle } }
        },
        new()
        {
            Title = "Healing at Lourdes",
            Country = "France",
            Century = 19,
            Image = "img",
            Description = "desc",
            MarkdownPath = "md",
            Slug = "lourdes",
            Tags = new() { new Tag { Name = "Healing", TagType = TagType.Miracle } }
        },
        new()
        {
            Title = "Eucharistic Miracle of Lanciano",
            Country = "Italy",
            Century = 8,
            Image = "img",
            Description = "desc",
            MarkdownPath = "md",
            Slug = "lanciano",
            Tags = new() { new Tag { Name = "Eucharistic", TagType = TagType.Miracle } }
        }
    };

    [Fact]
    public async Task GetAllAsync_ShouldReturnAll()
    {
        var repo = CreateRepository(out _);
        var result = await repo.GetAllAsync(new MiracleFilters());
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByCountry()
    {
        var repo = CreateRepository(out _);
        var result = await repo.GetAllAsync(new MiracleFilters { Country = "Italy" });
        Assert.Single(result.Items);
        Assert.Equal("Italy", result.Items.First().Country);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnCorrectMiracle()
    {
        var repo = CreateRepository(out _);
        var miracle = await repo.GetBySlugAsync("lourdes");
        Assert.NotNull(miracle);
        Assert.Equal("France", miracle!.Country);
    }

    [Fact]
    public async Task GetCountriesAsync_ShouldReturnDistinctCountries()
    {
        var repo = CreateRepository(out _);
        var countries = await repo.GetCountriesAsync();
        Assert.Equal(3, countries.Count);
        Assert.Contains("Portugal", countries);
        Assert.Contains("Italy", countries);
        Assert.Contains("France", countries);
    }
}
