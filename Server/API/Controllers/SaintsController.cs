using Core.Interfaces;
using Core.Models;
using Core.Models.Filters;
using Core.Validation.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaintsController(
    ISaintsRepository saintsRepository,
    ISaintsService saintsService,
    UserManager<AppUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllSaints([FromQuery] SaintFilters filters)
    {
        var saints = await saintsRepository.GetAllAsync(filters);
        return Ok(saints);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> GetById(int id)
    {
        var saint = await saintsRepository.GetByIdAsync(id);
        return saint is null ? NotFound() : Ok(saint);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetSaintBySlug([FromRoute][SafeSlug] string slug)
    {
        var saint = await saintsRepository.GetBySlugAsync(slug);
        return saint is null ? NotFound() : Ok(saint);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateSaint([FromBody] NewSaintDto newSaint)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var created = await saintsService.CreateSaintAsync(newSaint, user.Id);
        if (!created.HasValue)
            return Conflict("A saint with the same name already exists.");

        return CreatedAtAction(nameof(GetById), new { id = created.Value }, null);
    }

    [Authorize]
    [HttpPut("{id:int:min(1)}")]
    public async Task<IActionResult> UpdateSaint(int id, [FromBody] NewSaintDto updatedSaint)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var updated = await saintsService.UpdateSaintAsync(id, updatedSaint, user.Id);
        return updated ? NoContent() : NotFound();
    }

    [Authorize]
    [HttpDelete("{id:int:min(1)}")]
    public async Task<IActionResult> DeleteSaint(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var saint = await saintsRepository.GetByIdAsync(id);
        if (saint is null)
            return NotFound();

        await saintsService.DeleteSaintAsync(id, user.Id);
        return Ok();
    }

    [HttpGet("countries")]
    public async Task<IActionResult> GetSaintCountries()
    {
        var countries = await saintsRepository.GetCountriesAsync();
        return Ok(countries);
    }

    [HttpGet("of-the-day")]
    public async Task<IActionResult> GetSaintsOfTheDay()
    {
        var saints = await saintsRepository.GetSaintsOfTheDayAsync(DateOnly.FromDateTime(DateTime.UtcNow));

        return saints.Any() ? Ok(saints) : NoContent();
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingFeasts()
    {
        var saints = await saintsRepository.GetUpcomingFeasts(DateOnly.FromDateTime(DateTime.UtcNow));
        return saints.Any() ? Ok(saints) : NoContent();
    }
}