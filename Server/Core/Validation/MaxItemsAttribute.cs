using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Core.Validation.Attributes;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class MaxItemsAttribute : SafeStringValidationAttribute
{
    public int Max { get; }

    public MaxItemsAttribute(int max)
    {
        if (max <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(max),
                "Max must be greater than zero."
            );

        Max = max;
    }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not IEnumerable collection)
        {
            return CreateValidationError(
                validationContext,
                "must be a collection."
            );
        }

        var count = 0;

        foreach (var _ in collection)
        {
            count++;

            if (count > Max)
            {
                return CreateValidationError(
                    validationContext,
                    $"must contain at most {Max} items."
                );
            }
        }

        return ValidationResult.Success;
    }
}
