using System.Net;
using System.Security.Claims;
using Core.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.Common;

/// <summary>
/// Base class for integration tests. Provides real HTTP client, real database, and full middleware pipeline.
/// Use this for end-to-end testing across controllers, services, and repositories.
/// NOTE: This needs to be customized per test class since Common project can't reference Program.cs
/// </summary>
public abstract class IntegrationTestBase
{
    protected HttpClient? HttpClient { get; set; }
    protected IServiceScope? ServiceScope { get; set; }
    protected DataContext? DbContext { get; set; }
    protected UserManager<AppUser>? UserManager { get; set; }

    /// <summary>
    /// Creates and saves a test user, returning the user ID.
    /// </summary>
    protected async Task<string> CreateUserAsync(string email = "test@test.com", string password = "TestPassword123!")
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await UserManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        return user.Id;
    }

    /// <summary>
    /// Gets an HttpClient with authentication headers for a specific user.
    /// </summary>
    protected async Task<HttpClient> CreateAuthenticatedClientAsync(string email = "test@test.com", string password = "TestPassword123!")
    {
        var user = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
        var createResult = await UserManager!.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException("Failed to create test user");
        }

        var client = new HttpClient();

        // Add authentication token
        var token = await GenerateJwtTokenAsync(user);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    /// <summary>
    /// Saves changes to the database context.
    /// </summary>
    protected async Task SaveChangesAsync()
        => await DbContext.SaveChangesAsync();

    /// <summary>
    /// Clears all data from the database (useful for test isolation).
    /// </summary>
    protected async Task ClearDatabaseAsync()
    {
        foreach (var entity in DbContext.Model.GetEntityTypes())
        {
            await DbContext.Database.ExecuteSqlRawAsync($"DELETE FROM \"{entity.GetTableName()}\"");
        }
    }

    /// <summary>
    /// Generates a JWT token for a user (stub implementation - should be overridden if real JWT is needed).
    /// </summary>
    protected Task<string> GenerateJwtTokenAsync(AppUser user)
    {
        // Stub: Returns a placeholder token. Override in derived test class if real JWT is needed.
        return Task.FromResult("test-jwt-token-placeholder");
    }

    /// <summary>
    /// Helper assertion for HTTP response success.
    /// </summary>
    protected void AssertSuccess(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode,
            $"Expected successful response but got {response.StatusCode}: {response.Content.ReadAsStringAsync().Result}");
    }

    /// <summary>
    /// Helper assertion for HTTP response status code.
    /// </summary>
    protected void AssertStatusCode(HttpStatusCode expectedCode, HttpResponseMessage response)
    {
        Assert.Equal(expectedCode, response.StatusCode);
    }
}
