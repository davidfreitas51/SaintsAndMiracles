using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class ChangeEmailRequestDto
{
    [Required]
    [EmailAddress]
    [SafeEmail]
    public required string NewEmail { get; set; }
}
