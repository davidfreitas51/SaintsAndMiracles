using Core.DTOs;

namespace Tests.Common.Builders;

/// <summary>
/// Fluent builder for creating LoginDto test objects.
/// </summary>
public class LoginDtoBuilder
{
    private string _email = "test@test.com";
    private string _password = "SecurePass123!";
    private bool _rememberMe;

    /// <summary>
    /// Creates a default LoginDto with standard test values.
    /// </summary>
    public static LoginDtoBuilder Default()
        => new();

    /// <summary>
    /// Creates a valid LoginDto with minimal required fields.
    /// </summary>
    public static LoginDtoBuilder Valid()
        => new LoginDtoBuilder()
            .WithEmail("valid@test.com")
            .WithPassword("ValidPassword123!");

    /// <summary>
    /// Creates a LoginDto with invalid email (for negative tests).
    /// </summary>
    public static LoginDtoBuilder InvalidEmail()
        => new LoginDtoBuilder()
            .WithEmail("not-an-email");

    /// <summary>
    /// Creates a LoginDto with missing password (for negative tests).
    /// </summary>
    public static LoginDtoBuilder MissingPassword()
        => new LoginDtoBuilder()
            .WithPassword("");

    /// <summary>
    /// Creates a LoginDto with missing email (for negative tests).
    /// </summary>
    public static LoginDtoBuilder MissingEmail()
        => new LoginDtoBuilder()
            .WithEmail("");

    public LoginDtoBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public LoginDtoBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public LoginDtoBuilder WithRememberMe(bool rememberMe = true)
    {
        _rememberMe = rememberMe;
        return this;
    }

    /// <summary>
    /// Builds and returns the LoginDto instance.
    /// </summary>
    public LoginDto Build()
    {
        return new LoginDto
        {
            Email = _email,
            Password = _password,
            RememberMe = _rememberMe
        };
    }
}
