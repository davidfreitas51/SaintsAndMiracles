using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property)]
public class NotOnlyNumbersAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) return ValidationResult.Success;
        if (value is not string text) return ValidationResult.Success;

        if (Regex.IsMatch(text, @"^\d+$"))
        {
            return new ValidationResult(ErrorMessage ?? "Cannot be only numbers.");
        }

        return ValidationResult.Success;
    }
}