using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using Core.Validation;

namespace Core.Validation.Attributes;

public sealed class SafeSlugAttribute : SafeStringValidationAttribute
{
    private static readonly Regex SlugRegex = new(
        @"^(?!-)(?!.*--)[a-z0-9-]+(?<!-)$",
        RegexOptions.Compiled);

    private const string InvalidSlugMessage =
        "must contain only lowercase letters, numbers and hyphens, " +
        "cannot start or end with hyphen, and cannot contain consecutive hyphens.";

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
                InvalidSlugMessage
            );

        return ValidationResult.Success;
    }
}
