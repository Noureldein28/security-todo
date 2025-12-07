using SecureTodo.Application.DTOs;

namespace SecureTodo.Application.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default);
    Task<LoginResponseDto?> GoogleLoginAsync(string googleId, string email, string username, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
}
