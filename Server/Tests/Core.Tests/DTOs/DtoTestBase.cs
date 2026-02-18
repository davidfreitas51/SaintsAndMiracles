using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Base class for DTO validation tests providing common helper methods.
/// </summary>
public abstract class DtoTestBase
{
    /// <summary>
    /// Validates a DTO and returns validation results.
    /// </summary>
    protected static IList<ValidationResult> Validate(object dto)
    {
        return ModelValidationHelper.Validate(dto);
    }

    /// <summary>
    /// Asserts that validation passed (no errors).
    /// </summary>
    protected static void AssertValid(IList<ValidationResult> results)
    {
        Assert.Empty(results);
    }

    /// <summary>
    /// Asserts that validation failed with at least one error.
    /// </summary>
    protected static void AssertInvalid(IList<ValidationResult> results)
    {
        Assert.NotEmpty(results);
    }

    /// <summary>
    /// Asserts that validation failed for a specific property.
    /// </summary>
    protected static void AssertInvalidProperty(IList<ValidationResult> results, string propertyName)
    {
        Assert.Contains(results, r => r.MemberNames.Contains(propertyName));
    }

    /// <summary>
    /// Asserts that validation failed for a specific property with a specific error message fragment.
    /// </summary>
    protected static void AssertInvalidProperty(IList<ValidationResult> results, string propertyName, string errorMessageFragment)
    {
        var propertyErrors = results.Where(r => r.MemberNames.Contains(propertyName)).ToList();
        Assert.NotEmpty(propertyErrors);
        Assert.Contains(propertyErrors, e => e.ErrorMessage?.Contains(errorMessageFragment, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Asserts that validation failed with a specific number of errors.
    /// </summary>
    protected static void AssertErrorCount(IList<ValidationResult> results, int expectedCount)
    {
        Assert.Equal(expectedCount, results.Count);
    }

    /// <summary>
    /// Asserts that validation passed for a specific property (no errors for that property).
    /// </summary>
    protected static void AssertValidProperty(IList<ValidationResult> results, string propertyName)
    {
        Assert.DoesNotContain(results, r => r.MemberNames.Contains(propertyName));
    }

    /// <summary>
    /// Gets all error messages for a specific property.
    /// </summary>
    protected static IEnumerable<string> GetPropertyErrors(IList<ValidationResult> results, string propertyName)
    {
        return results
            .Where(r => r.MemberNames.Contains(propertyName))
            .Select(r => r.ErrorMessage ?? string.Empty);
    }
}
