using Core.Interfaces.Services;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Infrastructure.Tests.Services;

public class AccountTokensServiceTests
{
    private DataContext CreateContext([System.Runtime.CompilerServices.CallerMemberName] string testName = "")
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase($"AccountTokensTestDb_{testName}_{Guid.NewGuid()}")
            .Options;
        return new DataContext(options);
    }

    private Mock<ITokenService> CreateTokenServiceMock()
    {
        var mock = new Mock<ITokenService>();
        mock.Setup(ts => ts.GenerateClearToken(It.IsAny<int>())).Returns("clear-token");
        mock.Setup(ts => ts.HashTokenBase64(It.IsAny<string>())).Returns<string>(t => $"hash-{t}");
        mock.Setup(ts => ts.VerifyToken(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((provided, hash) => hash == $"hash-{provided}");
        return mock;
    }

    [Fact]
    public async Task GenerateInviteAsync_ShouldCreateTokenAndReturnClearToken()
    {
        using var context = CreateContext();
        var tokenService = CreateTokenServiceMock();
        var service = new AccountTokensService(context, tokenService.Object);

        var clearToken = await service.GenerateInviteAsync("Admin");

        Assert.Equal("clear-token", clearToken);
        var tokenInDb = context.AccountTokens.FirstOrDefault();
        Assert.NotNull(tokenInDb);
        Assert.Equal("Admin", tokenInDb!.Role);
        Assert.False(tokenInDb.IsUsed);
        Assert.StartsWith("hash-", tokenInDb.Hash);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnTrueForValidToken()
    {
        using var context = CreateContext();
        var tokenService = CreateTokenServiceMock();
        var service = new AccountTokensService(context, tokenService.Object);

        var token = await service.GenerateInviteAsync("Admin");

        var isValid = await service.ValidateAsync(token);

        Assert.True(isValid);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnFalseForInvalidToken()
    {
        using var context = CreateContext();
        var tokenService = CreateTokenServiceMock();
        var service = new AccountTokensService(context, tokenService.Object);

        await service.GenerateInviteAsync("Admin");

        var isValid = await service.ValidateAsync("wrong-token");

        Assert.False(isValid);
    }

    [Fact]
    public async Task ConsumeAsync_ShouldMarkTokenAsUsed()
    {
        using var context = CreateContext();
        var tokenService = CreateTokenServiceMock();
        var service = new AccountTokensService(context, tokenService.Object);

        var token = await service.GenerateInviteAsync("Admin");

        var consumed = await service.ConsumeAsync(token);

        Assert.True(consumed);
        var tokenInDb = context.AccountTokens.First();
        Assert.True(tokenInDb.IsUsed);

        // Trying to consume again returns false
        var consumedAgain = await service.ConsumeAsync(token);
        Assert.False(consumedAgain);
    }

    [Fact]
    public async Task GetValidTokenAsync_ShouldReturnTokenObject()
    {
        using var context = CreateContext();
        var tokenService = CreateTokenServiceMock();
        var service = new AccountTokensService(context, tokenService.Object);

        var token = await service.GenerateInviteAsync("Admin");

        var tokenObj = await service.GetValidTokenAsync(token);

        Assert.NotNull(tokenObj);
        Assert.Equal("Admin", tokenObj!.Role);
        Assert.False(tokenObj.IsUsed);
    }

    [Fact]
    public async Task ExpiredOrUsedToken_ShouldNotBeValid()
    {
        using var context = CreateContext();
        var tokenService = CreateTokenServiceMock();
        var service = new AccountTokensService(context, tokenService.Object);

        var token = await service.GenerateInviteAsync("Admin");

        // Expire token manually
        var tokenInDb = context.AccountTokens.First();
        tokenInDb.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await context.SaveChangesAsync();

        var isValid = await service.ValidateAsync(token);
        Assert.False(isValid);

        // Reset and consume it
        tokenInDb.ExpiresAtUtc = DateTime.UtcNow.AddHours(1);
        tokenInDb.IsUsed = true;
        await context.SaveChangesAsync();

        var isValidUsed = await service.ValidateAsync(token);
        Assert.False(isValidUsed);
    }
}
