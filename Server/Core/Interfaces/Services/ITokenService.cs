namespace Core.Interfaces.Services;

public interface ITokenService
{
    string GenerateClearToken(int size = 32);
    string HashTokenBase64(string clearToken);
    bool VerifyToken(string providedToken, string storedHash);
}
