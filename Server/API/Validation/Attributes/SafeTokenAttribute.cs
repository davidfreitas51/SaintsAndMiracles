using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Attributes;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class SafeTokenAttribute : ValidationAttribute
{
    private static readonly Regex _safeTokenRegex =
        new(@"^[A-Za-z0-9\-_]+$", RegexOptions.Compiled);

    public int MinLength { get; init; } = 32;
    public int MaxLength { get; init; } = 128;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string token)
            return new ValidationResult("SafeToken can only be applied to string fields.");

        token = token.Trim();

        if (token.Length < MinLength || token.Length > MaxLength)
        {
            return Error(validationContext,
                $"must be between {MinLength} and {MaxLength} characters.");
        }

        foreach (var ch in token)
        {
            if (char.IsWhiteSpace(ch) || char.IsControl(ch))
            {
                return Error(validationContext, "must not contain whitespace or control characters.");
            }
        }

        if (!_safeTokenRegex.IsMatch(token))
        {
            return Error(validationContext, "contains invalid characters.");
        }

        return ValidationResult.Success;
    }

    private ValidationResult Error(ValidationContext context, string message)
        => new(ErrorMessage ?? $"{context.MemberName} {message}");
}
