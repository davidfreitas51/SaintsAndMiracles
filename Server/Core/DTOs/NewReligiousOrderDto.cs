using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class NewReligiousOrderDto
{
    [Required]
    [SafeText]
    [StringLength(100, MinimumLength = 3)]
    public required string Name { get; set; }
}
