using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagsController(ITagsRepository tagsRepository, ITagsService tagsService, UserManager<AppUser> userManager) : ControllerBase
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
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var tag = await tagsService.CreateTagAsync(dto, user.Id);

        return tag is null ? BadRequest() : CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] NewTagDto dto)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var success = await tagsService.UpdateTagAsync(id, dto, user.Id);

        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var success = await tagsService.DeleteTagAsync(id, user.Id);

        return success ? Ok() : NotFound();
    }
}
