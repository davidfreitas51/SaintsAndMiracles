using System.Security.Cryptography;
using System.Text;
using Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class TokenService(ILogger<TokenService> logger) : ITokenService
{
    public string GenerateClearToken(int size = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(size);
        var token = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        logger.LogDebug("Clear token generated successfully. Size={Size}", size);
        return token;
    }

    public string HashTokenBase64(string clearToken)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(clearToken);
        var hash = sha.ComputeHash(bytes);
        var result = Convert.ToBase64String(hash);

        logger.LogDebug("Token hashed successfully");
        return result;
    }

    public bool VerifyToken(string providedToken, string storedHash)
    {
        var hashOfProvided = HashTokenBase64(providedToken);

        var result = CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(hashOfProvided),
            Convert.FromBase64String(storedHash));

        if (result)
        {
            logger.LogDebug("Token verification successful");
        }
        else
        {
            logger.LogWarning("Token verification failed: Hash mismatch");
        }

        return result;
    }
}
