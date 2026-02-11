using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace Core.Validation.Attributes;

public sealed class SafeSlugAttribute : SafeStringValidationAttribute
{
    private static readonly Regex SlugRegex = new(
        @"^(?!-)(?!.*--)[a-z0-9-]+(?<!-)$",
        RegexOptions.Compiled);

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string slug)
            return CreateValidationError(
                validationContext,
                "must be a string."
            );

        if (string.IsNullOrWhiteSpace(slug))
            return CreateValidationError(
                validationContext,
                "cannot be empty."
            );

        if (!SlugRegex.IsMatch(slug))
            return CreateValidationError(
                validationContext,
                "invalid format"
            );

        if (slug.Length > 150)
            return CreateValidationError(
                validationContext,
                "invalid format"
            );
        return ValidationResult.Success;
    }
}
