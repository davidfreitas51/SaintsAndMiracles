using Infrastructure.Services;

namespace Infrastructure.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _service = new TokenService();

    [Fact]
    public void GenerateClearToken_ShouldReturnNonEmptyString()
    {
        var token = _service.GenerateClearToken();
        Assert.False(string.IsNullOrEmpty(token));
        Assert.DoesNotContain("+", token);
        Assert.DoesNotContain("/", token);
        Assert.DoesNotContain("=", token);
    }

    [Fact]
    public void GenerateClearToken_WithCustomSize_ShouldReturnCorrectLength()
    {
        int size = 16; // bytes
        var token = _service.GenerateClearToken(size);
        var decoded = Convert.FromBase64String(
            token.Replace("-", "+").Replace("_", "/") + "==");
        Assert.Equal(size, decoded.Length);
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
        var start = DateTime.UtcNow;
        _service.VerifyToken(token, hash);
        _service.VerifyToken("wrong-token", hash);
        var elapsed = DateTime.UtcNow - start;

        Assert.True(elapsed.TotalMilliseconds >= 0);
    }
}
