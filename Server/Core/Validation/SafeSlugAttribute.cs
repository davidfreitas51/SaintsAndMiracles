using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Core.Validation.Attributes;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class SafeSlugAttribute : ValidationAttribute
{
    private static readonly Regex SlugRegex = new(
        @"^(?!-)(?!.*--)[a-z0-9-]+(?<!-)$",
        RegexOptions.Compiled);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string slug)
            return new ValidationResult("Slug must be a string.");

        if (string.IsNullOrWhiteSpace(slug))
            return new ValidationResult("Slug cannot be empty.");

        if (!SlugRegex.IsMatch(slug))
        {
            return new ValidationResult(
                ErrorMessage ??
                "Slug must contain only lowercase letters, numbers and hyphens, " +
                "cannot start or end with hyphen, and cannot contain consecutive hyphens.");
        }

        return ValidationResult.Success;
    }
}
