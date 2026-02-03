using System.ComponentModel.DataAnnotations;

namespace Core.Validation.Attributes;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class SafePathAttribute : SafeStringValidationAttribute
{
    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string path)
            return CreateValidationError(
                validationContext,
                "can only be applied to string fields."
            );

        if (string.IsNullOrWhiteSpace(path))
            return ValidationResult.Success;

        path = path.Replace('\\', '/');

        if (Path.IsPathRooted(path))
        {
            return CreateValidationError(
                validationContext,
                "must be a relative path."
            );
        }

        if (path.Contains(".."))
        {
            return CreateValidationError(
                validationContext,
                "must not contain '..'."
            );
        }

        if (path.Contains(':'))
        {
            return CreateValidationError(
                validationContext,
                "must not contain ':' characters."
            );
        }

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return CreateValidationError(
                validationContext,
                "contains invalid path characters."
            );
        }

        return ValidationResult.Success;
    }
}
