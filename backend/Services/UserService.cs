using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Data.Entities;
using backend.DTOs.User;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace backend.Services;

public class UserService: IUserService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public UserService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    private string GenerateAccessToken(int userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyStr = _configuration["Jwt:Key"];
        var key = Encoding.UTF8.GetBytes(keyStr!);

        var tokenDescriptior = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )            
        };

        var token = tokenHandler.CreateToken(tokenDescriptior);
        return tokenHandler.WriteToken(token);
    }

    private async Task<string> GenerateRefreshToken(int userId)
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        var newTokenString = Convert.ToBase64String(randomBytes);

        var newRefreshToken = new RefreshToken
        {
            UserId = userId,
            TokenString = newTokenString,
            IsActive = true,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return newTokenString;
    }

    public async Task RevokeToken(string refreshTokenString)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenString == refreshTokenString);
        if (refreshToken == null)
        {
            return;
        }

        refreshToken.IsActive = false;
        refreshToken.RevokedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<RefreshResponseDto?> Refresh(string refreshTokenString)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenString == refreshTokenString);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return null;
        }

        await RevokeToken(refreshTokenString);

        if (refreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        var newAccessToken = GenerateAccessToken(refreshToken.UserId);
        var newRefreshToken = await GenerateRefreshToken(refreshToken.UserId);

        return new RefreshResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task<LoginResponseDto?> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);
        if(user == null)
        {
            return null;
        }
        if(!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
            return null;
        }

        var accessToken = GenerateAccessToken(user.UserId);
        var refreshToken = await GenerateRefreshToken(user.UserId);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<RegisterResponseDto?> Register(RegisterDto dto)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
        {
            return null;
        }

        var hashedPass = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var newUser = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Password = hashedPass
        };


        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        var userResponse = new RegisterResponseDto
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserId = newUser.UserId
        };
        return userResponse;
    }

    public async Task<bool> ChangeEmail(int userId, ChangeEmailDto dto)
    {
        var user = await _context.Users
            .FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
        {
            return false;
        }

        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email == dto.NewEmail);
        if (emailTaken)
        {
            return false;
        }

        user.Email = dto.NewEmail;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePassword(int userId, ChangePasswordDto dto)
    {
        var user = await _context.Users
            .FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
        {
            return false;
        }

        var newHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        user.Password = newHash;
        await _context.SaveChangesAsync();
        return true;
    }
}