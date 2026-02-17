using Common;
using Core.Enums;
using Core.Models;
using Core.Models.Filters;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.Data;

public class TagsRepositoryTests
{
    private DataContext CreateContext([System.Runtime.CompilerServices.CallerMemberName] string testName = "")
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: $"TagsTestDb_{testName}_{Guid.NewGuid()}")
            .Options;

        return new DataContext(options);
    }

    private DummyCacheService CreateCache() => new DummyCacheService();

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResults()
    {
        using var context = CreateContext();
        var cache = CreateCache();
        var repo = new TagsRepository(context, cache, NullLogger<TagsRepository>.Instance);

        // Seed 10 tags
        for (int i = 1; i <= 10; i++)
        {
            context.Tags.Add(new Tag { Name = $"Tag{i}", TagType = TagType.Prayer });
        }
        await context.SaveChangesAsync();

        var filters = new EntityFilters { Page = 2, PageSize = 3 };
        var result = await repo.GetAllAsync(filters);

        Assert.NotNull(result);
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(3, result.Items.Count());
        Assert.True(result.Items.First().Name.CompareTo(result.Items.Last().Name) < 0);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectTag()
    {
        using var context = CreateContext();
        var cache = CreateCache();
        var repo = new TagsRepository(context, cache, NullLogger<TagsRepository>.Instance);

        var tag = new Tag { Name = "TestTag", TagType = TagType.Saint };
        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        var fetched = await repo.GetByIdAsync(tag.Id);

        Assert.NotNull(fetched);
        Assert.Equal(tag.Name, fetched!.Name);
        Assert.Equal(tag.TagType, fetched.TagType);
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnCorrectTags()
    {
        using var context = CreateContext();
        var cache = CreateCache();
        var repo = new TagsRepository(context, cache, NullLogger<TagsRepository>.Instance);

        var tags = new List<Tag>
        {
            new() { Name = "Tag1", TagType = TagType.Prayer },
            new() { Name = "Tag2", TagType = TagType.Saint }
        };
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync();

        var ids = tags.Select(t => t.Id).ToList();
        var fetched = await repo.GetByIdsAsync(ids);

        Assert.Equal(2, fetched.Count);
        Assert.Contains(fetched, t => t.Name == "Tag1");
        Assert.Contains(fetched, t => t.Name == "Tag2");
    }

    [Fact]
    public async Task CreateAsync_ShouldAddTag()
    {
        using var context = CreateContext();
        var cache = CreateCache();
        var repo = new TagsRepository(context, cache, NullLogger<TagsRepository>.Instance);

        var tag = new Tag { Name = "NewTag", TagType = TagType.Prayer };
        var created = await repo.CreateAsync(tag);

        Assert.True(created);
        Assert.Single(context.Tags);
        Assert.Equal("NewTag", context.Tags.First().Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyTag()
    {
        using var context = CreateContext();
        var cache = CreateCache();
        var repo = new TagsRepository(context, cache, NullLogger<TagsRepository>.Instance);

        var tag = new Tag { Name = "OldName", TagType = TagType.Prayer };
        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        tag.Name = "UpdatedName";
        var updated = await repo.UpdateAsync(tag);

        Assert.True(updated);
        Assert.Equal("UpdatedName", context.Tags.First().Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTag()
    {
        using var context = CreateContext();
        var cache = CreateCache();
        var repo = new TagsRepository(context, cache, NullLogger<TagsRepository>.Instance);

        var tag = new Tag { Name = "ToDelete", TagType = TagType.Saint };
        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        await repo.DeleteAsync(tag.Id);

        Assert.Empty(context.Tags);
    }
}
