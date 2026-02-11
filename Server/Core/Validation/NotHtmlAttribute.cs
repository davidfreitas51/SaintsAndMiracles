using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Core.Validation.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class NotHtmlAttribute : SafeStringValidationAttribute
{
    private static readonly Regex HtmlTagRegex =
        new(@"<\s*\/?\s*\w+[^>]*>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string text)
            return CreateValidationError(validationContext, "Invalid data type.");

        if (HtmlTagRegex.IsMatch(text))
            return CreateValidationError(
                validationContext,
                "invalid format"
            );

        return ValidationResult.Success;
    }
}
