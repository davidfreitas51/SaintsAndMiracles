using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

/// <summary>
/// Tests for MatchPropertyAttribute validation.
/// Ensures a property value matches another property value (e.g., Password == ConfirmPassword).
/// </summary>
public class MatchPropertyAttributeTests : ValidationTestBase
{
    private class PasswordModel
    {
        public string Password { get; set; } = "Secret123!";

        [MatchProperty("Password")]
        public string ConfirmPassword { get; set; } = "Secret123!";
    }

    private class NullablePasswordModel
    {
        public string? Password { get; set; }

        [MatchProperty("Password")]
        public string? ConfirmPassword { get; set; }
    }

    private class EmailModel
    {
        public string Email { get; set; } = "user@example.com";

        [MatchProperty("Email")]
        public string ConfirmEmail { get; set; } = "user@example.com";
    }

    private class InvalidReferenceModel
    {
        [MatchProperty("DoesNotExist")]
        public string Field { get; set; } = "Value";
    }

    private class CaseSensitiveModel
    {
        public string Value { get; set; } = "TestValue";

        [MatchProperty("Value")]
        public string ConfirmValue { get; set; } = "TestValue";
    }

    // ==================== MATCHING VALUES ====================

    [Fact]
    public void Should_Pass_PasswordsMatch()
    {
        var model = new PasswordModel
        {
            Password = "Secret123!",
            ConfirmPassword = "Secret123!"
        };

        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_EmailsMatch()
    {
        var model = new EmailModel
        {
            Email = "user@example.com",
            ConfirmEmail = "user@example.com"
        };

        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_EmptyStringsMatch()
    {
        var model = new PasswordModel
        {
            Password = "",
            ConfirmPassword = ""
        };

        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_LongStringsMatch()
    {
        string longValue = new string('A', 500);
        var model = new PasswordModel
        {
            Password = longValue,
            ConfirmPassword = longValue
        };

        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_ComplexPasswordsMatch()
    {
        var model = new PasswordModel
        {
            Password = "P@ssw0rd!#$%^&*()_+-=[]{}|;':,.<>?",
            ConfirmPassword = "P@ssw0rd!#$%^&*()_+-=[]{}|;':,.<>?"
        };

        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== NON-MATCHING VALUES ====================

    [Fact]
    public void Should_Fail_PasswordsDoNotMatch()
    {
        var model = new PasswordModel
        {
            Password = "Secret123!",
            ConfirmPassword = "WrongPassword"
        };

        var results = Validate(model);

        AssertInvalid(results, nameof(PasswordModel.ConfirmPassword), "must match");
    }

    [Fact]
    public void Should_Fail_EmailsDoNotMatch()
    {
        var model = new EmailModel
        {
            Email = "user@example.com",
            ConfirmEmail = "other@example.com"
        };

        var results = Validate(model);

        AssertInvalid(results, nameof(EmailModel.ConfirmEmail), "must match");
    }

    [Fact]
    public void Should_Fail_CaseDifference()
    {
        var model = new CaseSensitiveModel
        {
            Value = "TestValue",
            ConfirmValue = "testvalue"
        };

        var results = Validate(model);

        AssertInvalid(results, nameof(CaseSensitiveModel.ConfirmValue), "must match");
    }

    [Fact]
    public void Should_Fail_OneExtraCharacter()
    {
        var model = new PasswordModel
        {
            Password = "Secret123",
            ConfirmPassword = "Secret123!"
        };

        var results = Validate(model);

        AssertInvalid(results, nameof(PasswordModel.ConfirmPassword), "must match");
    }

    [Fact]
    public void Should_Fail_WhitespaceDifference()
    {
        var model = new PasswordModel
        {
            Password = "Secret123",
            ConfirmPassword = "Secret123 "
        };

        var results = Validate(model);

        AssertInvalid(results, nameof(PasswordModel.ConfirmPassword), "must match");
    }

    // ==================== NULL VALUES ====================

    [Fact]
    public void Should_Pass_BothNull()
    {
        var model = new NullablePasswordModel
        {
            Password = null,
            ConfirmPassword = null
        };

        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Fail_OneNull()
    {
        var model = new NullablePasswordModel
        {
            Password = "Secret123!",
            ConfirmPassword = null
        };

        var results = Validate(model);

        AssertInvalid(results, nameof(NullablePasswordModel.ConfirmPassword), "must match");
    }

    [Fact]
    public void Should_Fail_OtherNull()
    {
        var model = new NullablePasswordModel
        {
            Password = null,
            ConfirmPassword = "Secret123!"
        };

        var results = Validate(model);

        AssertInvalid(results, nameof(NullablePasswordModel.ConfirmPassword), "must match");
    }

    // ==================== INVALID REFERENCES ====================

    [Fact]
    public void Should_Fail_NonExistentProperty()
    {
        var model = new InvalidReferenceModel();

        var results = Validate(model);

        Assert.Single(results);
        Assert.Contains(nameof(InvalidReferenceModel.Field), results[0].MemberNames);
        Assert.Equal("Unknown property 'DoesNotExist'.", results[0].ErrorMessage);
    }
}
