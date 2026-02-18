using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Core.Tests.Validation;

/// <summary>
/// Base class for validation attribute tests providing common helper methods.
/// </summary>
public abstract class ValidationTestBase
{
    /// <summary>
    /// Validates a model and returns validation results.
    /// </summary>
    protected static IList<ValidationResult> Validate(object model)
    {
        return ModelValidationHelper.Validate(model);
    }

    /// <summary>
    /// Validates a single value using the specified attribute.
    /// </summary>
    protected static ValidationResult? ValidateValue(ValidationAttribute attribute, object? value, string memberName = "TestField")
    {
        var context = new ValidationContext(new { TestField = value })
        {
            MemberName = memberName
        };

        return attribute.GetValidationResult(value, context);
    }

    /// <summary>
    /// Asserts that validation passed (no errors).
    /// </summary>
    protected static void AssertValid(IList<ValidationResult> results)
    {
        Assert.Empty(results);
    }

    /// <summary>
    /// Asserts that validation failed with a single error.
    /// </summary>
    protected static void AssertInvalid(IList<ValidationResult> results, string? memberName = null, string? errorMessageFragment = null)
    {
        Assert.Single(results);

        if (memberName != null)
        {
            Assert.Contains(memberName, results[0].MemberNames);
        }

        if (errorMessageFragment != null)
        {
            Assert.Contains(errorMessageFragment, results[0].ErrorMessage);
        }
    }

    /// <summary>
    /// Asserts that a validation result is success.
    /// </summary>
    protected static void AssertValidationSuccess(ValidationResult? result)
    {
        Assert.Equal(ValidationResult.Success, result);
    }

    /// <summary>
    /// Asserts that a validation result is failure with optional message check.
    /// </summary>
    protected static void AssertValidationFailure(ValidationResult? result, string? errorMessageFragment = null)
    {
        Assert.NotNull(result);

        if (errorMessageFragment != null)
        {
            Assert.Contains(errorMessageFragment, result!.ErrorMessage);
        }
    }
}
