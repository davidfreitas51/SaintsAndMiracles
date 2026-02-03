using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Core.Validation.Attributes;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class MaxItemsAttribute : ValidationAttribute
{
    public int Max { get; }

    public MaxItemsAttribute(int max)
    {
        if (max <= 0)
            throw new ArgumentOutOfRangeException(nameof(max), "Max must be greater than zero.");

        Max = max;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not IEnumerable collection)
        {
            return new ValidationResult(
                ErrorMessage ?? "MaxItems can only be applied to collections.");
        }

        int count = 0;
        foreach (var _ in collection)
        {
            count++;
            if (count > Max)
            {
                return new ValidationResult(
                    ErrorMessage ??
                    $"The field {validationContext.MemberName} must contain at most {Max} items.");
            }
        }

        return ValidationResult.Success;
    }
}
