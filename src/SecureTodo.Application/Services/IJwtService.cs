using SecureTodo.Application.DTOs;

namespace SecureTodo.Application.Services;

/// <summary>
/// JWT token service interface
/// </summary>
public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string email);
    string GenerateRefreshToken();
    Guid? ValidateToken(string token);
}
