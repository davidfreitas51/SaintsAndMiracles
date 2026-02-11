using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class ResetPasswordDto
{
    [Required]
    [EmailAddress]
    [SafeEmail]
    public required string Email { get; set; }

    [Required]
    [SafeText]
    public required string Token { get; set; }

    [Required]
    public required string NewPassword { get; set; }
}
