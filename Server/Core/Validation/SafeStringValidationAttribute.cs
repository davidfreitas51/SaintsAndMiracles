using System.ComponentModel.DataAnnotations;

namespace Core.Validation;

public abstract class SafeStringValidationAttribute : ValidationAttribute
{
    protected ValidationResult CreateValidationError(
        ValidationContext context,
        string message
    )
        => new ValidationResult(
            ErrorMessage ?? $"{context.MemberName} {message}",
            new[] { context.MemberName! }
        );
}
