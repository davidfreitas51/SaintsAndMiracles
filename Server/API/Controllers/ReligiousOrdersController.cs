using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/religious-orders")]
[ApiController]
public class ReligiousOrdersController(IReligiousOrdersRepository ordersRepository) : ControllerBase
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
        var order = new ReligiousOrder { Name = dto.Name };

        var success = await ordersRepository.CreateAsync(order);
        return success
            ? CreatedAtAction(nameof(GetById), new { id = order.Id }, order)
            : BadRequest();
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] NewReligiousOrderDto dto)
    {
        var order = await ordersRepository.GetByIdAsync(id);
        if (order is null)
            return NotFound();

        order.Name = dto.Name;
        var success = await ordersRepository.UpdateAsync(order);
        return success ? NoContent() : BadRequest();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await ordersRepository.GetByIdAsync(id);
        if (order is null)
            return NotFound();

        await ordersRepository.DeleteAsync(id);
        return Ok();
    }
}
