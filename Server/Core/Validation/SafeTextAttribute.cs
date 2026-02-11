using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Core.Validation.Attributes;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class SafeTextAttribute : SafeStringValidationAttribute
{
    private static readonly Regex HtmlTagRegex =
        new(@"<[^>]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HtmlEntityRegex =
        new(@"&(#\d+|#x[a-f0-9]+|\w+);", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string text)
        {
            return CreateValidationError(
                validationContext,
                "SafeText can only be applied to string fields."
            );
        }

        if (string.IsNullOrWhiteSpace(text))
            return ValidationResult.Success;

        if (HtmlTagRegex.IsMatch(text))
        {
            return CreateValidationError(
                validationContext,
                ErrorMessage ??
                $"invalid format"
            );
        }

        if (HtmlEntityRegex.IsMatch(text))
        {
            return CreateValidationError(
                validationContext,
                ErrorMessage ??
                $"invalid format"
            );
        }

        return ValidationResult.Success;
    }
}
