using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class DashboardController(ISaintsRepository saintsRepository, IMiraclesRepository miraclesRepository, IPrayersRepository prayersRepository) : ControllerBase
{
    [HttpGet("saints")]
    public async Task<IActionResult> TotalSaints()
    {
        var totalSaints = await saintsRepository.GetTotalSaintsAsync();
        return Ok(totalSaints);
    }

    [HttpGet("miracles")]
    public async Task<IActionResult> TotalMiracles()
    {
        var totalMiracles = await miraclesRepository.GetTotalMiraclesAsync();
        return Ok(totalMiracles);
    }

    [HttpGet("prayers")]
    public async Task<IActionResult> TotalPrayers()
    {
        var totalPrayers = await prayersRepository.GetTotalPrayersAsync();
        return Ok(totalPrayers);
    }

    [HttpGet("users")]
    public async Task<IActionResult> TotalUsers()
    {
        return Ok(0);
    }
}
