using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Core.Validation;

namespace Core.Validation.Attributes;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class SafeTokenAttribute : SafeStringValidationAttribute
{
    private static readonly Regex _safeTokenRegex = new(@"^[A-Za-z0-9\-_]+$", RegexOptions.Compiled);

    public int MinLength { get; init; } = 32;
    public int MaxLength { get; init; } = 128;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string token)
            return CreateValidationError(validationContext, "can only be applied to string fields.");

        token = token.Trim();

        if (token.Length < MinLength || token.Length > MaxLength)
            return CreateValidationError(validationContext, $"must be between {MinLength} and {MaxLength} characters.");

        if (token.Any(char.IsWhiteSpace) || token.Any(char.IsControl))
            return CreateValidationError(validationContext, "must not contain whitespace or control characters.");

        if (!_safeTokenRegex.IsMatch(token))
            return CreateValidationError(validationContext, "contains invalid characters.");

        return ValidationResult.Success;
    }
}
