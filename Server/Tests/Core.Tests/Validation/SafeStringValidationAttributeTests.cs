using System.ComponentModel.DataAnnotations;
using Core.Validation;
using Xunit;

namespace Core.Tests.Validation;

public class SafeStringValidationAttributeTests
{
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

    [Fact]
    public void CreateValidationError_Should_ReturnValidationResult_WithMemberName()
    {
        var attr = new TestSafeStringAttribute();

        var dto = new { Property = "value" };
        var context = new ValidationContext(dto)
        {
            MemberName = "Property"
        };

        var result = attr.TestCreateValidationError(context, "is invalid");

        Assert.NotNull(result);
        Assert.Equal("Property is invalid", result!.ErrorMessage);
        Assert.Single(result.MemberNames);
        Assert.Contains("Property", result.MemberNames);
    }

    [Fact]
    public void CreateValidationError_Should_Use_CustomErrorMessage_WhenProvided()
    {
        var attr = new TestSafeStringAttribute
        {
            ErrorMessage = "Custom error"
        };

        var dto = new { Property = "value" };
        var context = new ValidationContext(dto)
        {
            MemberName = "Property"
        };

        var result = attr.TestCreateValidationError(context, "ignored message");

        Assert.NotNull(result);
        Assert.Equal("Custom error", result!.ErrorMessage);
        Assert.Single(result.MemberNames);
        Assert.Contains("Property", result.MemberNames);
    }
}
