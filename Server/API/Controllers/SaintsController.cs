using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SaintsController(
    ISaintsRepository saintsRepository,
    ISaintsService saintsService) : ControllerBase
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
        var created = await saintsService.CreateSaintAsync(newSaint);
        if (!created.HasValue)
            return Conflict("A saint with the same name already exists.");
        return CreatedAtAction(nameof(GetById), new { id = created.Value }, null);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateSaint(int id, [FromBody] NewSaintDto updatedSaint)
    {
        var updated = await saintsService.UpdateSaintAsync(id, updatedSaint);
        if (!updated)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteSaint(int id)
    {
        var saint = await saintsRepository.GetByIdAsync(id);
        if (saint is null)
            return NotFound();

        await saintsService.DeleteFilesAsync(saint.Slug);
        await saintsRepository.DeleteAsync(id);
        return Ok();
    }

    [HttpGet("countries")]
    public async Task<IActionResult> GetSaintCountries()
    {
        var countries = await saintsRepository.GetCountriesAsync();
        return Ok(countries);
    }
}
