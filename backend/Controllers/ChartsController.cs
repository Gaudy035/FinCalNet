using System.Security.Claims;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
public class ChartsController: ControllerBase
{
    private readonly IChartsService _chartsService;

    public ChartsController(IChartsService chartsService)
    {
        _chartsService = chartsService;
    }

    [Authorize]
    [HttpGet("get_stats")]
    public async Task<IActionResult> GetStats()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized("Nieautoryzwoany dostep");
        }

        var stats = await _chartsService.GetStats(userId);

        return Ok(stats);
    }

    [Authorize]
    [HttpGet("get_summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized("Nieautoryzwoany dostep");
        }
        
        var summary = await _chartsService.GetSummary(userId);

        return Ok(summary);
    }
}