using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Core.Validation.Attributes;

public sealed class ImageSourceAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
            return ValidationResult.Success;

        if (s.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return ValidationResult.Success;

        if (Regex.IsMatch(s, @"^[a-zA-Z]:\\|^\\\\"))
            return new ValidationResult("Invalid image source.");

        if (s.Contains(".."))
            return new ValidationResult("Invalid image source.");

        if (s.Contains(":"))
            return new ValidationResult("Invalid image source.");

        if (s.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            return new ValidationResult("Invalid image source.");

        return ValidationResult.Success;
    }
}