using System.Security.Cryptography;
using System.Text;
using Core.Interfaces.Services;

namespace Infrastructure.Services;

public class TokenService : ITokenService
{
    public string GenerateClearToken(int size = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(size);

        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    public string HashTokenBase64(string clearToken)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(clearToken);
        var hash = sha.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }

    public bool VerifyToken(string providedToken, string storedHash)
    {
        var hashOfProvided = HashTokenBase64(providedToken);

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(hashOfProvided),
            Convert.FromBase64String(storedHash));
    }
}
