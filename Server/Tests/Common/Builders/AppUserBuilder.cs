using Core.Models;

namespace Tests.Common.Builders;

/// <summary>
/// Fluent builder for creating AppUser test objects.
/// </summary>
public class AppUserBuilder
{
    private string _id = Guid.NewGuid().ToString();
    private string _userName = "testuser@test.com";
    private string _email = "testuser@test.com";
    private bool _emailConfirmed = true;
    private string? _firstName = "John";
    private string? _lastName = "Doe";
    private string? _phoneNumber;
    private bool _phoneNumberConfirmed;
    private bool _twoFactorEnabled;
    private DateTimeOffset? _lockoutEnd;
    private bool _lockoutEnabled = true;
    private int _accessFailedCount;

    /// <summary>
    /// Creates a default user with standard test values.
    /// </summary>
    public static AppUserBuilder Default()
        => new();

    public AppUserBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public AppUserBuilder WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }

    public AppUserBuilder WithEmail(string email)
    {
        _email = email;
        _userName = email;
        return this;
    }

    public AppUserBuilder WithEmailConfirmed(bool confirmed = true)
    {
        _emailConfirmed = confirmed;
        return this;
    }

    public AppUserBuilder WithFirstName(string? firstName)
    {
        _firstName = firstName;
        return this;
    }

    public AppUserBuilder WithLastName(string? lastName)
    {
        _lastName = lastName;
        return this;
    }

    public AppUserBuilder WithFullName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public AppUserBuilder WithPhoneNumber(string phoneNumber, bool confirmed = false)
    {
        _phoneNumber = phoneNumber;
        _phoneNumberConfirmed = confirmed;
        return this;
    }

    public AppUserBuilder WithTwoFactorEnabled(bool enabled = true)
    {
        _twoFactorEnabled = enabled;
        return this;
    }

    public AppUserBuilder WithLockedOut(DateTime? until = null)
    {
        _lockoutEnd = until == null ? null : new DateTimeOffset(until.Value);
        return this;
    }

    public AppUserBuilder WithAccessFailedCount(int count)
    {
        _accessFailedCount = count;
        return this;
    }

    /// <summary>
    /// Builds and returns the AppUser instance.
    /// </summary>
    public AppUser Build()
    {
        return new AppUser
        {
            Id = _id,
            UserName = _userName,
            Email = _email,
            EmailConfirmed = _emailConfirmed,
            FirstName = _firstName,
            LastName = _lastName,
            PhoneNumber = _phoneNumber,
            PhoneNumberConfirmed = _phoneNumberConfirmed,
            TwoFactorEnabled = _twoFactorEnabled,
            LockoutEnd = _lockoutEnd,
            LockoutEnabled = _lockoutEnabled,
            AccessFailedCount = _accessFailedCount
        };
    }
}
