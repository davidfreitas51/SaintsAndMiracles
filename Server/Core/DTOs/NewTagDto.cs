using System.ComponentModel.DataAnnotations;
using Core.Enums;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class NewTagDto
{
    [Required]
    [SafeText]
    [StringLength(100, MinimumLength = 3)]
    public required string Name { get; set; }

    [Required]
    public required TagType TagType { get; set; }
}
