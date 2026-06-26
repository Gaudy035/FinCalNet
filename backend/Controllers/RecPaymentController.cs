using System.Security.Claims;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
public class RecPaymentController: ControllerBase
{
    private readonly IRecPaymentService _recPaymentService;

    public RecPaymentController(IRecPaymentService recPaymentService)
    {
        _recPaymentService = recPaymentService;
    }

    [Authorize]
    [HttpGet("get_recurring")]
    public async Task<IActionResult> GetRecPayments()
    {
        var userIdstr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userIdstr == null || !int.TryParse(userIdstr, out int userId))
        {
            return Unauthorized("Nieautoryzwoany dostep");
        }
        var recPayments = await _recPaymentService.GetRecPayments(userId);
        return Ok(recPayments);
    }
}