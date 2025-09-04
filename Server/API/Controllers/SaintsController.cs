using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var saint = await saintsRepository.GetByIdAsync(id);
        return saint is null ? NotFound() : Ok(saint);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetSaintBySlug(string slug)
    {
        var saint = await saintsRepository.GetBySlugAsync(slug);
        return saint is null ? NotFound() : Ok(saint);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateSaint([FromBody] NewSaintDto newSaint)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var created = await saintsService.CreateSaintAsync(newSaint, user.Id);
        if (!created.HasValue)
            return Conflict("A saint with the same name already exists.");

        return CreatedAtAction(nameof(GetById), new { id = created.Value }, null);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateSaint(int id, [FromBody] NewSaintDto updatedSaint)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var updated = await saintsService.UpdateSaintAsync(id, updatedSaint, user.Id);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
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
    public async Task<IActionResult> GetSaintOfTheDay()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var saint = await saintsRepository.GetSaintOfTheDayAsync(today);

        if (saint is null)
            return NoContent();

        return Ok(saint);
    }
}
