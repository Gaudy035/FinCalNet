using System.Security.Claims;
using backend.DTOs.Payment;
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
        if (userIdstr == null || !int.TryParse(userIdstr, out int userId))
        {
            return Unauthorized("Nieautoryzwoany dostep");
        }
        var recPayments = await _recPaymentService.GetRecPayments(userId);
        return Ok(recPayments);
    }

    [Authorize]
    [HttpPost("add_recurring")]
    public async Task<IActionResult> AddRecPayment([FromBody] RecPaymentCreateDto dto)
    {
        var userIdstr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdstr == null || !int.TryParse(userIdstr, out int userId))
        {
            return Unauthorized("Nieautoryzwoany dostep");
        }
        
        var newRecPayment = await _recPaymentService.AddRecPayment(userId, dto);
        
        return Ok(newRecPayment);
    }

    [Authorize]
    [HttpPut("modify_recurring/{recPaymentId:int}")]
    public async Task<IActionResult> ModifyRecPayment([FromRoute] int recPaymentId, [FromBody] RecPaymentModifyDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized("Nieautoryzwoany dostep");
        }

        var modifiedRecPayment = await _recPaymentService.ModifyRecPayment(recPaymentId, userId, dto);

        return Ok(modifiedRecPayment);
    }
}