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
public class MiraclesController(
    IMiraclesRepository miraclesRepository,
    IMiraclesService miraclesService,
    UserManager<AppUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllMiracles([FromQuery] MiracleFilters filters)
    {
        var miracles = await miraclesRepository.GetAllAsync(filters);
        return Ok(miracles);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var miracle = await miraclesRepository.GetByIdAsync(id);
        return miracle is null ? NotFound() : Ok(miracle);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetMiracleBySlug(string slug)
    {
        var miracle = await miraclesRepository.GetBySlugAsync(slug);
        return miracle is null ? NotFound() : Ok(miracle);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateMiracle([FromBody] NewMiracleDto newMiracle)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var created = await miraclesService.CreateMiracleAsync(newMiracle, user.Id);
        if (!created.HasValue)
            return Conflict("A miracle with the same name already exists.");

        return CreatedAtAction(nameof(GetById), new { id = created.Value }, null);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateMiracle(int id, [FromBody] NewMiracleDto updatedMiracle)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var updated = await miraclesService.UpdateMiracleAsync(id, updatedMiracle, user.Id);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteMiracle(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var miracle = await miraclesRepository.GetByIdAsync(id);
        if (miracle is null)
            return NotFound();

        await miraclesService.DeleteMiracleAsync(miracle.Slug, user.Id);
        return Ok();
    }

    [HttpGet("countries")]
    public async Task<IActionResult> GetMiracleCountries()
    {
        var countries = await miraclesRepository.GetCountriesAsync();
        return Ok(countries);
    }
}
