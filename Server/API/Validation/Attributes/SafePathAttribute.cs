using System.ComponentModel.DataAnnotations;

namespace Attributes;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class SafePathAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string path)
            return new ValidationResult("SafePath can only be applied to string fields.");

        if (string.IsNullOrWhiteSpace(path))
            return ValidationResult.Success;

        path = path.Replace('\\', '/');

        if (Path.IsPathRooted(path))
        {
            return Error(validationContext, "must be a relative path.");
        }

        if (path.Contains(".."))
        {
            return Error(validationContext, "must not contain '..'.");
        }

        if (path.Contains(':'))
        {
            return Error(validationContext, "must not contain ':' characters.");
        }

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return Error(validationContext, "contains invalid path characters.");
        }

        return ValidationResult.Success;
    }

    private ValidationResult Error(ValidationContext context, string message)
        => new(ErrorMessage ?? $"{context.MemberName} {message}");
}
