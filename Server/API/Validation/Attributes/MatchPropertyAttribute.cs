using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Core.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class MatchPropertyAttribute : ValidationAttribute
{
    private readonly string _otherPropertyName;

    public MatchPropertyAttribute(string otherPropertyName)
    {
        _otherPropertyName = otherPropertyName;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance == null)
            return ValidationResult.Success;

        PropertyInfo? otherProperty = validationContext.ObjectType.GetProperty(_otherPropertyName);

        if (otherProperty == null)
        {
            return new ValidationResult(
                $"Unknown property '{_otherPropertyName}'."
            );
        }

        object? otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

        if (!Equals(value, otherValue))
        {
            string errorMessage = ErrorMessage
                ?? $"{validationContext.MemberName} must match {_otherPropertyName}.";

            return new ValidationResult(errorMessage);
        }

        return ValidationResult.Success;
    }
}
