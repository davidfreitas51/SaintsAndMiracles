using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Core.Validation.Attributes;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class PersonNameAttribute : ValidationAttribute
{
    private static readonly Regex _regex =
        new(@"^[\p{L}\p{M}'\- ]+$", RegexOptions.Compiled);

    public int MinLength { get; init; } = 2;
    public int MaxLength { get; init; } = 100;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string name)
            return new ValidationResult("PersonName can only be applied to string fields.");

        name = name.Trim();

        if (name.Length < MinLength || name.Length > MaxLength)
        {
            return Error(validationContext,
                $"must be between {MinLength} and {MaxLength} characters.");
        }

        if (!_regex.IsMatch(name))
        {
            return Error(validationContext,
                "contains invalid characters for a person name.");
        }

        return ValidationResult.Success;
    }

    private ValidationResult Error(ValidationContext context, string message)
        => new(ErrorMessage ?? $"{context.MemberName} {message}");
}
