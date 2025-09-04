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
public class PrayersController(
    IPrayersRepository prayersRepository,
    IPrayersService prayersService,
    UserManager<AppUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllPrayers([FromQuery] PrayerFilters prayerFilters)
    {
        var prayers = await prayersRepository.GetAllAsync(prayerFilters);
        return Ok(prayers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var prayer = await prayersRepository.GetByIdAsync(id);
        return prayer is null ? NotFound() : Ok(prayer);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetPrayerBySlug(string slug)
    {
        var prayer = await prayersRepository.GetBySlugAsync(slug);
        return prayer is null ? NotFound() : Ok(prayer);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePrayer([FromBody] NewPrayerDto newPrayer)
    {
        var userId = userManager.GetUserId(User);
        var created = await prayersService.CreatePrayerAsync(newPrayer, userId);

        if (!created.HasValue)
            return Conflict("A prayer with the same title already exists.");

        return CreatedAtAction(nameof(GetById), new { id = created.Value }, null);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePrayer(int id, [FromBody] NewPrayerDto updatedPrayer)
    {
        var userId = userManager.GetUserId(User);
        var updated = await prayersService.UpdatePrayerAsync(id, updatedPrayer, userId);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeletePrayer(int id)
    {
        var userId = userManager.GetUserId(User);
        var prayer = await prayersRepository.GetByIdAsync(id);
        if (prayer is null)
            return NotFound();

        await prayersService.DeletePrayerAsync(prayer.Slug, userId);
        return Ok();
    }

    [HttpGet("tags")]
    public async Task<IActionResult> GetPrayerTags()
    {
        var tags = await prayersRepository.GetTagsAsync();
        return Ok(tags);
    }
}
