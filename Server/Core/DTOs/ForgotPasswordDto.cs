using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    [SafeEmail]
    public required string Email { get; set; }
}
