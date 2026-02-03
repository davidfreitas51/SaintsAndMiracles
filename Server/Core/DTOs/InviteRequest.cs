using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

public class InviteRequest
{
    [Required]
    [SafeText]
    public string Role { get; set; } = "Admin";
}