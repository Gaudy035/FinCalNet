using System.Security.Claims;
using backend.DTOs.User;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
public class UserController: ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    private void CreateTokenCookie(string refreshToken)
    {
        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var responseDto = await _userService.Register(dto);
        if (responseDto == null)
        {
            return BadRequest(new { detail = "Email jest juz zajety" });
        }
        return Created(string.Empty, responseDto);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var responseDto = await _userService.Login(dto);
        if (responseDto == null)
        {
            return BadRequest(new { detail = "Nieprawidlowy email lub haslo" });
        }

        var refreshToken = responseDto.RefreshToken;

        CreateTokenCookie(refreshToken);
        
        return Ok(responseDto);
    }

    [Authorize]
    [HttpPut("update_email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userIdStr == null || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized(new { detail = "Nieautoryzwoany dostep" });
        }

        var success = await _userService.ChangeEmail(userId, dto);
        return success
            ? Ok(new { message = "Email zmieniony pomyslnie" })
            : BadRequest(new { detail = "Niepoprawne obecne haslo lub zajety adres email" });
    }

    [Authorize]
    [HttpPut("update_password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {   
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized(new { detail = "Nieautoryzwoany dostep" });
        }

        var success = await _userService.ChangePassword(userId, dto);
        return success
            ? Ok(new { message = "Haslo zmienione pomyslnie" })
            : BadRequest(new { detail = "Nieprawidlowe haslo" });
    }
}