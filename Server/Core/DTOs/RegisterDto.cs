using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class RegisterDto
{
    [Required]
    [PersonName]
    public required string FirstName { get; set; }

    [Required]
    [PersonName]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    [SafeEmail]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    [MatchProperty(nameof(Password))]
    public required string ConfirmPassword { get; set; }

    [Required]
    [SafeToken]
    public required string InviteToken { get; set; }
}
