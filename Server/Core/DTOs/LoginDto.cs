using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    [SafeEmail]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }

    public bool RememberMe { get; set; }
}
