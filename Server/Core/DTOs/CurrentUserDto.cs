using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class CurrentUserDto
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
}
