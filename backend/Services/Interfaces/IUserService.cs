using backend.DTOs.User;

namespace backend.Services;

public interface IUserService
{
    Task<LoginResponseDto?> Login(LoginDto dto);

    Task<RegisterResponseDto?> Register(RegisterDto dto);
    
    Task<bool> ChangePassword(int userId, ChangePasswordDto dto);
    
    Task<bool> ChangeEmail(int userId, ChangeEmailDto dto);

    Task<RefreshResponseDto?> Refresh(string refreshTokenString);
}