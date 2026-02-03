using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public class ChangePasswordDto
{
    [Required]
    public required string CurrentPassword { get; set; }
    
    [Required]
    public required string NewPassword { get; set; }
}
