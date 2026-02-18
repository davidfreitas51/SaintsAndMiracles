using Core.Enums;
using Core.Models;
using Core.Models.Filters;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Common;

namespace Infrastructure.Tests.Data;

public class SaintsRepositoryTests
{
    private SaintsRepository CreateRepository(out DataContext context)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        context = new DataContext(options);
        context.Database.EnsureCreated();

        var cache = new DummyCacheService();
        var repo = new SaintsRepository(context, cache, NullLogger<SaintsRepository>.Instance);

        context.Saints.AddRange(GetSeedData());
        context.SaveChanges();

        return repo;
    }

    private static List<Saint> GetSeedData() => new()
    {
        new()
        {
            Name = "Francis of Assisi",
            Country = "Italy",
            Century = 12,
            Image = "/saints/francis-of-assisi/image.png",
            Description = "Founder of the Franciscans, known for his humility and love of creation.",
            MarkdownPath = "/saints/francis-of-assisi/markdown.md",
            Title = "The Poor Man of Assisi",
            FeastDay = new DateOnly(1, 10, 4),
            PatronOf = "Animals and Ecology",
            ReligiousOrder = new() { Name = "Franciscan (OFM)" },
            Slug = "francis-of-assisi",
            Tags = new()
            {
                new Tag { Name = "Founder", TagType = TagType.Saint },
                new Tag { Name = "Monk", TagType = TagType.Saint },
                new Tag { Name = "Saint of the Day", TagType = TagType.Saint }
            }
        },
        new()
        {
            Name = "Teresa of Ávila",
            Country = "Spain",
            Century = 16,
            Image = "/saints/teresa-of-avila/image.png",
            Description = "Mystic, writer, and reformer of the Carmelite Order.",
            MarkdownPath = "/saints/teresa-of-avila/markdown.md",
            Title = "The Mystical Doctor",
            FeastDay = new DateOnly(1, 10, 15),
            PatronOf = "Writers and Mystics",
            ReligiousOrder = new() { Name = "Carmelite" },
            Slug = "teresa-of-avila",
            Tags = new()
            {
                new Tag { Name = "Mystic", TagType = TagType.Saint },
                new Tag { Name = "Doctor of the Church", TagType = TagType.Saint },
                new Tag { Name = "Nun", TagType = TagType.Saint },
                new Tag { Name = "Saint of the Day", TagType = TagType.Saint }
            }
        },
        new()
        {
            Name = "Augustine of Hippo",
            Country = "Algeria",
            Century = 4,
            Image = "/saints/augustine-of-hippo/image.png",
            Description = "Theologian and philosopher, author of 'Confessions' and 'City of God'.",
            MarkdownPath = "/saints/augustine-of-hippo/markdown.md",
            Title = "Doctor of Grace",
            FeastDay = new DateOnly(1, 8, 28),
            PatronOf = "Theologians and Printers",
            Slug = "augustine-of-hippo",
            Tags = new()
            {
                new Tag { Name = "Philosopher", TagType = TagType.Saint },
                new Tag { Name = "Doctor of the Church", TagType = TagType.Saint },
                new Tag { Name = "Saint of the Day", TagType = TagType.Saint }
            }
        },
        new()
        {
            Name = "Joan of Arc",
            Country = "France",
            Century = 15,
            Image = "/saints/joan-of-arc/image.png",
            Description = "French heroine and martyr who led troops during the Hundred Years' War.",
            MarkdownPath = "/saints/joan-of-arc/markdown.md",
            Title = "The Maid of Orléans",
            FeastDay = new DateOnly(1, 5, 30),
            PatronOf = "France and Soldiers",
            Slug = "joan-of-arc",
            Tags = new()
            {
                new Tag { Name = "Martyr", TagType = TagType.Saint },
                new Tag { Name = "Heroine", TagType = TagType.Saint },
                new Tag { Name = "Saint of the Day", TagType = TagType.Saint }
            }
        },
        new()
        {
            Name = "Padre Pio",
            Country = "Italy",
            Century = 20,
            Image = "/saints/padre-pio/image.png",
            Description = "Known for his piety, miracles, and bearing the stigmata.",
            MarkdownPath = "/saints/padre-pio/markdown.md",
            FeastDay = new DateOnly(1, 9, 23),
            PatronOf = "Spiritual Healing",
            ReligiousOrder = new() { Name = "Capuchin Franciscan" },
            Slug = "padre-pio",
            Tags = new()
            {
                new Tag { Name = "Mystic", TagType = TagType.Saint },
                new Tag { Name = "Stigmatist", TagType = TagType.Saint },
                new Tag { Name = "Saint of the Day", TagType = TagType.Saint }
            }
        }
    };

    [Fact]
    public async Task GetAllAsync_ShouldReturnAll()
    {
        var repo = CreateRepository(out _);
        var result = await repo.GetAllAsync(new SaintFilters());
        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByCountry()
    {
        var repo = CreateRepository(out _);
        var result = await repo.GetAllAsync(new SaintFilters { Country = "Italy" });
        Assert.Equal(2, result.Items.Count());
        Assert.All(result.Items, s => Assert.Equal("Italy", s.Country));
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByCentury()
    {
        var repo = CreateRepository(out _);
        var result = await repo.GetAllAsync(new SaintFilters { Century = "15" });
        Assert.Single(result.Items);
        Assert.Equal("Joan of Arc", result.Items.First().Name);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnCorrectSaint()
    {
        var repo = CreateRepository(out _);
        var saint = await repo.GetBySlugAsync("teresa-of-avila");
        Assert.NotNull(saint);
        Assert.Equal("Spain", saint!.Country);
        Assert.Equal("Carmelite", saint.ReligiousOrder!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectSaint()
    {
        var repo = CreateRepository(out var context);
        var id = context.Saints.First().Id;
        var saint = await repo.GetByIdAsync(id);
        Assert.NotNull(saint);
        Assert.Equal("Francis of Assisi", saint!.Name);
    }

    [Fact]
    public async Task SlugExistsAsync_ShouldReturnTrueIfExists()
    {
        var repo = CreateRepository(out _);
        var exists = await repo.SlugExistsAsync("joan-of-arc");
        Assert.True(exists);
    }

    [Fact]
    public async Task GetCountriesAsync_ShouldReturnDistinctCountries()
    {
        var repo = CreateRepository(out _);
        var countries = await repo.GetCountriesAsync();
        Assert.Equal(4, countries.Count);
        Assert.Contains("Italy", countries);
        Assert.Contains("France", countries);
        Assert.Contains("Spain", countries);
        Assert.Contains("Algeria", countries);
    }

    [Fact]
    public async Task GetTotalSaintsAsync_ShouldReturnCorrectCount()
    {
        var repo = CreateRepository(out _);
        var total = await repo.GetTotalSaintsAsync();
        Assert.Equal(5, total);
    }

    [Fact]
    public async Task GetSaintsOfTheDayAsync_ShouldReturnCorrectSaint()
    {
        var repo = CreateRepository(out _);

        var saints = await repo.GetSaintsOfTheDayAsync(new DateOnly(1, 10, 4));

        Assert.NotNull(saints);
        Assert.Single(saints);
        Assert.Equal(
            "Francis of Assisi",
            saints[0].Name
        );
    }


    [Fact]
    public async Task CreateAsync_ShouldAddNewSaint()
    {
        var repo = CreateRepository(out var context);

        var newSaint = new Saint
        {
            Name = "Anthony of Padua",
            Country = "Italy",
            Century = 13,
            Image = "abc",
            MarkdownPath = "/123/123",
            Description = "Preacher and Doctor of the Church.",
            Slug = "anthony-of-padua",
            Tags = new() { new Tag { Name = "Preacher", TagType = TagType.Saint } }
        };

        var created = await repo.CreateAsync(newSaint);
        Assert.True(created);
        Assert.Equal(6, context.Saints.Count());
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingSaint()
    {
        var repo = CreateRepository(out var context);
        var saint = context.Saints.First(s => s.Name == "Joan of Arc");
        saint.Description = "Changed description.";
        var result = await repo.UpdateAsync(saint);
        Assert.True(result);
        var updated = await context.Saints.FindAsync(saint.Id);
        Assert.Equal("Changed description.", updated!.Description);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSaint()
    {
        var repo = CreateRepository(out var context);
        var id = context.Saints.First().Id;
        await repo.DeleteAsync(id);
        Assert.False(context.Saints.Any(s => s.Id == id));
    }

    [Fact]
    public async Task GetUpcomingFeasts_ShouldReturnSaintsOrderedByNextFeast()
    {
        var repo = CreateRepository(out _);
        var today = new DateOnly(2024, 5, 1);

        var result = await repo.GetUpcomingFeasts(today);

        Assert.NotEmpty(result);
        Assert.Equal("Joan of Arc", result[0].Name);
        Assert.Equal("Augustine of Hippo", result[1].Name);
        Assert.Equal("Padre Pio", result[2].Name);
        Assert.Equal("Francis of Assisi", result[3].Name);
        Assert.Equal("Teresa of Ávila", result[4].Name);
    }

    [Fact]
    public async Task GetUpcomingFeasts_ShouldRollOverToNextYearForPastFeasts()
    {
        var repo = CreateRepository(out _);
        var today = new DateOnly(2024, 12, 31);

        var result = await repo.GetUpcomingFeasts(today);

        Assert.NotEmpty(result);
        Assert.Equal("Joan of Arc", result.First().Name);
    }

    [Fact]
    public async Task GetUpcomingFeasts_ShouldRespectTakeLimit()
    {
        var repo = CreateRepository(out _);
        var today = new DateOnly(2024, 1, 1);

        var result = await repo.GetUpcomingFeasts(today, take: 3);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetUpcomingFeasts_ShouldIgnoreSaintsWithoutFeastDay()
    {
        var repo = CreateRepository(out var context);

        context.Saints.Add(new Saint
        {
            Name = "Test Saint Without Feast",
            Slug = "test-no-feast",
            Century = 10,
            Country = "Testland",
            Image = "image.png",
            Description = "",
            MarkdownPath = ""
        });

        context.SaveChanges();

        var today = new DateOnly(2024, 1, 1);
        var result = await repo.GetUpcomingFeasts(today);

        Assert.DoesNotContain(result, s => s.Name == "Test Saint Without Feast");
    }

}
