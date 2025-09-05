using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/religious-orders")]
[ApiController]
public class ReligiousOrdersController(
    IReligiousOrdersRepository ordersRepository,
    IReligiousOrdersService ordersService,
    UserManager<AppUser> userManager
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EntityFilters filters)
    {
        var orders = await ordersRepository.GetAllAsync(filters);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await ordersRepository.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] NewReligiousOrderDto dto)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var createdOrder = await ordersService.CreateReligiousOrderAsync(dto, user.Id);

        return createdOrder is not null
            ? CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder)
            : BadRequest();
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] NewReligiousOrderDto dto)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var updated = await ordersService.UpdateReligiousOrderAsync(id, dto, user.Id);

        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var deleted = await ordersService.DeleteReligiousOrderAsync(id, user.Id);

        return deleted ? Ok() : NotFound();
    }
}
