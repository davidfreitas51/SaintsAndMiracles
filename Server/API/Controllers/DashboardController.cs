using System.ComponentModel.DataAnnotations;
using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DashboardController(
    ISaintsRepository saintsRepository,
    IMiraclesRepository miraclesRepository,
    IPrayersRepository prayersRepository,
    IRecentActivityRepository recentActivityRepository,
    UserManager<AppUser> userManager) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = new DashboardSummaryDto
        {
            TotalSaints = await saintsRepository.GetTotalSaintsAsync(),
            TotalMiracles = await miraclesRepository.GetTotalMiraclesAsync(),
            TotalPrayers = await prayersRepository.GetTotalPrayersAsync(),
            TotalAccounts = await userManager.Users.CountAsync(u => u.EmailConfirmed)
        };

        return Ok(summary);
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentActivity(
        [FromQuery][Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery][Range(1, 100)] int pageSize = 10)
    {
        var pagedActivities =
            await recentActivityRepository.GetRecentActivitiesAsync(pageNumber, pageSize);

        var userIds = pagedActivities.Items
            .Where(a => a.UserId != null)
            .Select(a => a.UserId!)
            .Distinct()
            .ToList();

        var users = await userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        var items = pagedActivities.Items.Select(a => new
        {
            a.EntityName,
            a.EntityId,
            a.DisplayName,
            a.Action,
            a.CreatedAt,
            UserEmail = users.FirstOrDefault(u => u.Id == a.UserId)?.Email
        });

        return Ok(new
        {
            Items = items,
            pagedActivities.TotalCount,
            pagedActivities.PageNumber,
            pagedActivities.PageSize
        });
    }
}