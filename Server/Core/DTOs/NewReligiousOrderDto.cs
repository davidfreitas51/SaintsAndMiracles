using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class NewReligiousOrderDto
{
    [Required]
    [SafeText]
    public required string Name { get; set; }
}
