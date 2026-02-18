using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Tests.Common;

/// <summary>
/// Base class for repository tests. Provides in-memory database setup and common utilities.
/// Each test gets its own isolated database instance.
/// </summary>
public abstract class RepositoryTestBase
{
    /// <summary>
    /// Creates a fresh in-memory database context with EnsureCreated called.
    /// </summary>
    protected DataContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new DataContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Creates a cache service suitable for testing (in-memory, no Redis).
    /// </summary>
    protected ICacheService CreateTestCache()
        => new DummyCacheService();

    /// <summary>
    /// Helper to add and save an entity to the database.
    /// </summary>
    protected async Task<T> AddAndSaveAsync<T>(DataContext context, T entity) where T : class
    {
        context.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Helper to add multiple entities and save them.
    /// </summary>
    protected async Task AddAndSaveAsync<T>(DataContext context, params T[] entities) where T : class
    {
        context.AddRange(entities);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Helper to detach all entities from a context (useful after queries).
    /// </summary>
    protected void DetachAllEntities(DataContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            entry.State = EntityState.Detached;
        }
    }

    /// <summary>
    /// Gets the NullLogger for use in repository logging.
    /// </summary>
    protected ILogger<T> GetNullLogger<T>()
        => NullLogger<T>.Instance;

    /// <summary>
    /// Helper to assert an entity was tracked and modified.
    /// </summary>
    protected void AssertModified(DataContext context, object entity)
        => Assert.Equal(EntityState.Modified, context.Entry(entity).State);

    /// <summary>
    /// Helper to assert an entity was added to the database.
    /// </summary>
    protected void AssertAdded(DataContext context, object entity)
        => Assert.Equal(EntityState.Added, context.Entry(entity).State);
}
