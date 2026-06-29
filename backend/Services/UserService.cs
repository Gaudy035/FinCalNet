using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Data.Entities;
using backend.DTOs.User;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

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

    public string GenerateAccessToken(int userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyStr = _configuration["Jwt:Key"];
        var key = Encoding.UTF8.GetBytes(keyStr!);

        var tokenDescriptior = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )            
        };

        var token = tokenHandler.CreateToken(tokenDescriptior);
        return tokenHandler.WriteToken(token);
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

        var token = GenerateAccessToken(user.UserId);
        return new LoginResponseDto
        {
            AccessToken = token
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