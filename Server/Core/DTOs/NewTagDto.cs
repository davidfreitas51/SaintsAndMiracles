using System.ComponentModel.DataAnnotations;
using Core.Enums;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class NewTagDto
{
    [Required]
    [SafeText]
    public required string Name { get; set; }

    [Required]
    public required TagType TagType { get; set; }
}
