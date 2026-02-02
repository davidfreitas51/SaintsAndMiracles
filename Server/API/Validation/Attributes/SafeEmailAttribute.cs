using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace Attributes;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class SafeEmailAttribute : ValidationAttribute
{
    public int MaxLength { get; init; } = 254;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string email)
            return new ValidationResult("SafeEmail can only be applied to string fields.");

        email = email.Trim();

        if (email.Length == 0)
            return ValidationResult.Success;

        if (email.Length > MaxLength)
        {
            return Error(validationContext,
                $"must not exceed {MaxLength} characters.");
        }

        foreach (var ch in email)
        {
            if (char.IsControl(ch))
            {
                return Error(validationContext,
                    "contains invalid control characters.");
            }
        }

        if (email.Contains(' '))
        {
            return Error(validationContext,
                "must not contain spaces.");
        }

        try
        {
            var addr = new MailAddress(email);
            if (addr.Address != email)
            {
                return Error(validationContext,
                    "is not a valid email address.");
            }
        }
        catch
        {
            return Error(validationContext,
                "is not a valid email address.");
        }

        return ValidationResult.Success;
    }

    private ValidationResult Error(ValidationContext context, string message)
        => new(ErrorMessage ?? $"{context.MemberName} {message}");
}
