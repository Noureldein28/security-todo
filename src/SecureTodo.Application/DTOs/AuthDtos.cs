namespace SecureTodo.Application.DTOs;

/// <summary>
/// Authentication-related DTOs
/// </summary>

public record RegisterDto(
    string Username,
    string Email,
    string Password
);

public record LoginDto(
    string Email,
    string Password
);

public record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    UserDto User
);

public record RefreshTokenDto(
    string RefreshToken
);

public record GoogleLoginDto(
    string IdToken
);

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    DateTime CreatedAt
);
