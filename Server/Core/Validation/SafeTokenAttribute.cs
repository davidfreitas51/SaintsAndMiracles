using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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
            return CreateValidationError(validationContext, "invalid format");

        token = token.Trim();

        if (token.Length < MinLength || token.Length > MaxLength)
            return CreateValidationError(validationContext, $"invalid format.");

        if (token.Any(char.IsWhiteSpace) || token.Any(char.IsControl))
            return CreateValidationError(validationContext, "invalid format.");

        if (!_safeTokenRegex.IsMatch(token))
            return CreateValidationError(validationContext, "invalid format.");

        return ValidationResult.Success;
    }
}
