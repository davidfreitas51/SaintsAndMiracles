using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace API.Validation.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class NotHtmlAttribute : ValidationAttribute
{
    private static readonly Regex HtmlTagRegex =
        new(@"<\s*\/?\s*\w+[^>]*>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string text)
            return new ValidationResult("Invalid data type.");

        if (HtmlTagRegex.IsMatch(text))
        {
            return new ValidationResult(
                ErrorMessage ??
                $"{validationContext.DisplayName} must not contain HTML.");
        }

        return ValidationResult.Success;
    }
}
