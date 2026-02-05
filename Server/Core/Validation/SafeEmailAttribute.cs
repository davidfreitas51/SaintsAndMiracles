using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Core.Validation.Attributes;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class SafeEmailAttribute : SafeStringValidationAttribute
{
    public int MaxLength { get; init; } = 254;

    private static readonly Regex UnsafeCharsRegex = new(@"[<>&""']", RegexOptions.Compiled);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null || string.IsNullOrWhiteSpace(value as string))
            return CreateValidationError(validationContext, "cannot be null or empty.");

        string email = ((string)value).Trim();

        if (email.Length > MaxLength)
            return CreateValidationError(validationContext, $"must not exceed {MaxLength} characters.");

        if (email.Any(char.IsControl))
            return CreateValidationError(validationContext, "contains invalid control characters.");

        if (email.Contains(' '))
            return CreateValidationError(validationContext, "must not contain spaces.");

        if (UnsafeCharsRegex.IsMatch(email))
            return CreateValidationError(validationContext, "invalid format.");

        try
        {
            var addr = new MailAddress(email);
            var parts = addr.Address.Split('@');
            if (parts.Length != 2)
                return CreateValidationError(validationContext, "is not a valid email address.");

            string domain = parts[1];
            var domainParts = domain.Split('.');

            if (domainParts.Length < 2 || domainParts.Any(p => string.IsNullOrWhiteSpace(p)))
            {
                return CreateValidationError(validationContext, "is not a valid email address.");
            }
        }
        catch
        {
            return CreateValidationError(validationContext, "is not a valid email address.");
        }

        return ValidationResult.Success;
    }
}
