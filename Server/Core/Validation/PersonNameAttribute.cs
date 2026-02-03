using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Core.Validation.Attributes;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class PersonNameAttribute : SafeStringValidationAttribute
{
    private static readonly Regex Regex = new(
        @"^[\p{L}\p{M}'\- ]+$",
        RegexOptions.Compiled);

    public int MinLength { get; init; } = 2;
    public int MaxLength { get; init; } = 100;

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string name)
            return CreateValidationError(
                validationContext,
                "must be a string."
            );

        name = name.Trim();

        if (name.Length < MinLength || name.Length > MaxLength)
            return CreateValidationError(
                validationContext,
                $"must be between {MinLength} and {MaxLength} characters."
            );

        if (!Regex.IsMatch(name))
            return CreateValidationError(
                validationContext,
                "contains invalid characters for a person name."
            );

        return ValidationResult.Success;
    }
}
