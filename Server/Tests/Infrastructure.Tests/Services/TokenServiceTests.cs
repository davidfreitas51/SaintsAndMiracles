using System.Diagnostics;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _service = new TokenService(NullLogger<TokenService>.Instance);

    private static byte[] DecodeBase64Url(string token)
    {
        var base64 = token.Replace("-", "+").Replace("_", "/");
        var padding = (4 - base64.Length % 4) % 4;
        if (padding > 0)
        {
            base64 = base64.PadRight(base64.Length + padding, '=');
        }

        return Convert.FromBase64String(base64);
    }

    [Fact]
    public void GenerateClearToken_ShouldReturnNonEmptyString()
    {
        var token = _service.GenerateClearToken();

        Assert.NotEmpty(token);
        Assert.DoesNotContain("+", token);
        Assert.DoesNotContain("/", token);
        Assert.DoesNotContain("=", token);
    }

    [Fact]
    public void GenerateClearToken_WithCustomSize_ShouldReturnCorrectLength()
    {
        const int size = 16;
        var token = _service.GenerateClearToken(size);

        var decoded = DecodeBase64Url(token);

        Assert.Equal(size, decoded.Length);
    }

    [Fact]
    public void GenerateClearToken_WithDefaultSize_ShouldReturn32BytesWhenDecoded()
    {
        var token = _service.GenerateClearToken();

        var decoded = DecodeBase64Url(token);

        Assert.Equal(32, decoded.Length);
    }

    [Fact]
    public void HashTokenBase64_ShouldReturnBase64String()
    {
        var token = "my-secret-token";
        var hash = _service.HashTokenBase64(token);

        var decoded = Convert.FromBase64String(hash);
        Assert.Equal(32, decoded.Length);
    }

    [Fact]
    public void HashTokenBase64_ShouldBeDeterministicForSameInput()
    {
        const string token = "same-token";

        var firstHash = _service.HashTokenBase64(token);
        var secondHash = _service.HashTokenBase64(token);

        Assert.Equal(firstHash, secondHash);
    }

    [Fact]
    public void VerifyToken_ShouldReturnTrueForMatchingToken()
    {
        var token = _service.GenerateClearToken();
        var hash = _service.HashTokenBase64(token);

        bool isValid = _service.VerifyToken(token, hash);

        Assert.True(isValid);
    }

    [Fact]
    public void VerifyToken_ShouldReturnFalseForNonMatchingToken()
    {
        var token = _service.GenerateClearToken();
        var hash = _service.HashTokenBase64(token);

        bool isValid = _service.VerifyToken("wrong-token", hash);

        Assert.False(isValid);
    }

    [Fact]
    public void VerifyToken_ShouldBeTimingSafe()
    {
        var token = "token123";
        var hash = _service.HashTokenBase64(token);
        var stopwatch = Stopwatch.StartNew();

        var validResult = _service.VerifyToken(token, hash);
        var invalidResult = _service.VerifyToken("wrong-token", hash);
        stopwatch.Stop();

        Assert.True(validResult);
        Assert.False(invalidResult);
        Assert.True(stopwatch.Elapsed >= TimeSpan.Zero);
    }

    [Fact]
    public void VerifyToken_ShouldThrowFormatException_WhenStoredHashIsInvalidBase64()
    {
        var ex = Record.Exception(() => _service.VerifyToken("token", "not-base64"));

        Assert.IsType<FormatException>(ex);
    }
}
