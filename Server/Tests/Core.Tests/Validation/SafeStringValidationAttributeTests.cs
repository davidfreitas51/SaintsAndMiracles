using System.ComponentModel.DataAnnotations;
using Core.Validation;
using Xunit;

namespace Core.Tests.Validation;

/// <summary>
/// Tests for SafeStringValidationAttribute base class.
/// Tests the CreateValidationError helper method used by all string validation attributes.
/// </summary>
public class SafeStringValidationAttributeTests : ValidationTestBase
{
    /// <summary>
    /// Test implementation of SafeStringValidationAttribute for testing protected methods.
    /// </summary>
    private class TestSafeStringAttribute : SafeStringValidationAttribute
    {
        public ValidationResult? TestCreateValidationError(ValidationContext context, string message)
        {
            return CreateValidationError(context, message);
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }

    // ==================== CREATE VALIDATION ERROR ====================

    [Fact]
    public void CreateValidationError_Should_IncludeMemberName()
    {
        var attr = new TestSafeStringAttribute();
        var dto = new { Property = "value" };
        var context = new ValidationContext(dto) { MemberName = "Property" };

        var result = attr.TestCreateValidationError(context, "is invalid");

        Assert.NotNull(result);
        Assert.Equal("Property is invalid", result!.ErrorMessage);
        Assert.Single(result.MemberNames);
        Assert.Contains("Property", result.MemberNames);
    }

    [Fact]
    public void CreateValidationError_Should_CombineMemberNameAndMessage()
    {
        var attr = new TestSafeStringAttribute();
        var dto = new { Email = "test@example.com" };
        var context = new ValidationContext(dto) { MemberName = "Email" };

        var result = attr.TestCreateValidationError(context, "must be a valid email address");

        Assert.NotNull(result);
        Assert.Equal("Email must be a valid email address", result!.ErrorMessage);
        Assert.Contains("Email", result.MemberNames);
    }

    // ==================== CUSTOM ERROR MESSAGE ====================

    [Fact]
    public void CreateValidationError_Should_UseCustomErrorMessage()
    {
        var attr = new TestSafeStringAttribute { ErrorMessage = "Custom error" };
        var dto = new { Property = "value" };
        var context = new ValidationContext(dto) { MemberName = "Property" };

        var result = attr.TestCreateValidationError(context, "ignored message");

        Assert.NotNull(result);
        Assert.Equal("Custom error", result!.ErrorMessage);
        Assert.Single(result.MemberNames);
        Assert.Contains("Property", result.MemberNames);
    }

    [Fact]
    public void CreateValidationError_CustomError_Should_IgnoreDefaultMessage()
    {
        var attr = new TestSafeStringAttribute { ErrorMessage = "Field is required" };
        var dto = new { Username = "john" };
        var context = new ValidationContext(dto) { MemberName = "Username" };

        var result = attr.TestCreateValidationError(context, "must be alphanumeric");

        Assert.NotNull(result);
        Assert.Equal("Field is required", result!.ErrorMessage);
        Assert.DoesNotContain("alphanumeric", result!.ErrorMessage);
    }

    // ==================== DIFFERENT MEMBER NAMES ====================

    [Theory]
    [InlineData("Password", "must be strong")]
    [InlineData("Email", "is already taken")]
    [InlineData("Username", "contains invalid characters")]
    [InlineData("FirstName", "is too short")]
    [InlineData("LastName", "is too long")]
    public void CreateValidationError_Should_HandleVariousMemberNames(string memberName, string message)
    {
        var attr = new TestSafeStringAttribute();
        var dto = new { Field = "value" };
        var context = new ValidationContext(dto) { MemberName = memberName };

        var result = attr.TestCreateValidationError(context, message);

        Assert.NotNull(result);
        Assert.Equal($"{memberName} {message}", result!.ErrorMessage);
        Assert.Contains(memberName, result.MemberNames);
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void CreateValidationError_Should_HandleEmptyMessage()
    {
        var attr = new TestSafeStringAttribute();
        var dto = new { Property = "value" };
        var context = new ValidationContext(dto) { MemberName = "Property" };

        var result = attr.TestCreateValidationError(context, "");

        Assert.NotNull(result);
        Assert.Equal("Property ", result!.ErrorMessage);
        Assert.Contains("Property", result.MemberNames);
    }

    [Fact]
    public void CreateValidationError_Should_HandleLongMessage()
    {
        var attr = new TestSafeStringAttribute();
        var dto = new { Property = "value" };
        var context = new ValidationContext(dto) { MemberName = "Property" };
        var longMessage = "must contain at least 8 characters including uppercase, lowercase, numbers, and special characters";

        var result = attr.TestCreateValidationError(context, longMessage);

        Assert.NotNull(result);
        Assert.Equal($"Property {longMessage}", result!.ErrorMessage);
        Assert.Contains("Property", result.MemberNames);
    }

    [Fact]
    public void CreateValidationError_Should_HandleSpecialCharactersInMessage()
    {
        var attr = new TestSafeStringAttribute();
        var dto = new { Property = "value" };
        var context = new ValidationContext(dto) { MemberName = "Property" };

        var result = attr.TestCreateValidationError(context, "must not contain <>\"'&");

        Assert.NotNull(result);
        Assert.Equal("Property must not contain <>\"'&", result!.ErrorMessage);
    }

    // ==================== MEMBER NAME FORMAT ====================

    [Fact]
    public void CreateValidationError_Should_HandlePascalCaseMemberName()
    {
        var attr = new TestSafeStringAttribute();
        var dto = new { ConfirmPassword = "value" };
        var context = new ValidationContext(dto) { MemberName = "ConfirmPassword" };

        var result = attr.TestCreateValidationError(context, "must match Password");

        Assert.NotNull(result);
        Assert.Equal("ConfirmPassword must match Password", result!.ErrorMessage);
        Assert.Contains("ConfirmPassword", result.MemberNames);
    }

    [Fact]
    public void CreateValidationError_Should_HandleCamelCaseMemberName()
    {
        var attr = new TestSafeStringAttribute();
        var dto = new { emailAddress = "value" };
        var context = new ValidationContext(dto) { MemberName = "emailAddress" };

        var result = attr.TestCreateValidationError(context, "is invalid");

        Assert.NotNull(result);
        Assert.Equal("emailAddress is invalid", result!.ErrorMessage);
        Assert.Contains("emailAddress", result.MemberNames);
    }

    [Fact]
    public void CreateValidationError_Should_HandleNumbersInMemberName()
    {
        var attr = new TestSafeStringAttribute();
        var dto = new { Address1 = "value" };
        var context = new ValidationContext(dto) { MemberName = "Address1" };

        var result = attr.TestCreateValidationError(context, "is required");

        Assert.NotNull(result);
        Assert.Equal("Address1 is required", result!.ErrorMessage);
        Assert.Contains("Address1", result.MemberNames);
    }
}
