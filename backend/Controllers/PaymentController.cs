using System.Security.Claims;
using backend.DTOs.Payment;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

public class PaymentController: ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [Authorize]
    [HttpGet("transakcje")]
    public async Task<IActionResult> GetPayments()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized(new { detail = "Nieautoryzwoany dostep" });
        }
        var payments = await _paymentService.GetPayments(userId);
        return Ok(payments);
    }

    [Authorize]
    [HttpGet("wplywy")]
    public async Task<IActionResult> GetIncomes()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized(new { detail = "Nieautoryzwoany dostep" });
        }
        var payments = await _paymentService.GetIncomes(userId);
        return Ok(payments);
    }

    [Authorize]
    [HttpGet("wydatki")]
    public async Task<IActionResult> GetExpenses()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized(new { detail = "Nieautoryzwoany dostep" });
        }
        var payments = await _paymentService.GetExpenses(userId);
        return Ok(payments);
    }

    [Authorize]
    [HttpPost("add_payment")]
    public async Task<IActionResult> AddPayment([FromBody] PaymentCreateDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized(new { detail = "Nieautoryzwoany dostep" });
        }
        var newPayment = await _paymentService.AddPayment(userId, dto);
        return Ok(newPayment);
    }
}