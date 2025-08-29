using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagsController(ITagsRepository tagsRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EntityFilters filters)
    {
        var tags = await tagsRepository.GetAllAsync(filters);
        return Ok(tags);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tag = await tagsRepository.GetByIdAsync(id);
        return tag is null ? NotFound() : Ok(tag);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] NewTagDto dto)
    {
        var tag = new Tag { Name = dto.Name, TagType = dto.TagType };

        var success = await tagsRepository.CreateAsync(tag);
        return success ? CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag) : BadRequest();
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] NewTagDto dto)
    {
        var tag = await tagsRepository.GetByIdAsync(id);
        if (tag is null)
            return NotFound();

        tag.Name = dto.Name;
        var success = await tagsRepository.UpdateAsync(tag);
        return success ? NoContent() : BadRequest();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var tag = await tagsRepository.GetByIdAsync(id);
        if (tag is null)
            return NotFound();

        await tagsRepository.DeleteAsync(id);
        return Ok();
    }
}
