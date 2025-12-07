using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecureTodo.Application.DTOs;
using SecureTodo.Domain.Entities;
using SecureTodo.Domain.Interfaces;
using SecureTodo.Infrastructure.Security;
using SecureTodo.Shared.Constants;
using SecureTodo.Shared.Exceptions;

namespace SecureTodo.Application.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUnitOfWork unitOfWork,
        PasswordHasher passwordHasher,
        IJwtService jwtService,
        ILogger<AuthService> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        // Check if user exists
        if (await _unitOfWork.Users.EmailExistsAsync(dto.Email, cancellationToken))
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", dto.Email);
            throw new BusinessException(ErrorMessages.UserAlreadyExists);
        }

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email.ToLower(),
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New user registered: {Email}", dto.Email);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token
        await StoreRefreshTokenAsync(user.Id, refreshToken, cancellationToken);

        return new LoginResponseDto(
            accessToken,
            refreshToken,
            new UserDto(user.Id, user.Username, user.Email, user.CreatedAt)
        );
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("Login attempt for non-existent user: {Email}", dto.Email);
            throw new UnauthorizedException(ErrorMessages.InvalidCredentials);
        }

        // Check if user registered with Google
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            _logger.LogWarning("Password login attempt for Google OAuth user: {Email}", dto.Email);
            throw new BusinessException(ErrorMessages.GoogleUserNoPassword);
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for user: {Email}", dto.Email);
            throw new UnauthorizedException(ErrorMessages.InvalidCredentials);
        }

        _logger.LogInformation("Successful login: {Email}", dto.Email);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token
        await StoreRefreshTokenAsync(user.Id, refreshToken, cancellationToken);

        return new LoginResponseDto(
            accessToken,
            refreshToken,
            new UserDto(user.Id, user.Username, user.Email, user.CreatedAt)
        );
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default)
    {
        var storedToken = await _unitOfWork.Users.FindAsync(
            u => u.RefreshTokens.Any(rt => rt.Token == dto.RefreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow),
            cancellationToken);

        var user = storedToken.FirstOrDefault();
        if (user == null)
        {
            _logger.LogWarning("Invalid refresh token attempt");
            throw new UnauthorizedException(ErrorMessages.InvalidToken);
        }

        // Generate new tokens
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Revoke old refresh token and store new one
        var oldToken = user.RefreshTokens.First(rt => rt.Token == dto.RefreshToken);
        oldToken.IsRevoked = true;
        
        await StoreRefreshTokenAsync(user.Id, refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto(
            accessToken,
            refreshToken,
            new UserDto(user.Id, user.Username, user.Email, user.CreatedAt)
        );
    }

    public async Task<LoginResponseDto?> GoogleLoginAsync(string googleId, string email, string username, CancellationToken cancellationToken = default)
    {
        // Check if user exists by Google ID
        var user = await _unitOfWork.Users.GetByGoogleIdAsync(googleId, cancellationToken);

        if (user == null)
        {
            // Check if email exists (link accounts)
            user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
            
            if (user != null)
            {
                // Link Google account to existing user
                user.GoogleId = googleId;
                user.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // Create new user
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    Email = email.ToLower(),
                    GoogleId = googleId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        _logger.LogInformation("Successful Google login: {Email}", email);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await StoreRefreshTokenAsync(user.Id, refreshToken, cancellationToken);

        return new LoginResponseDto(
            accessToken,
            refreshToken,
            new UserDto(user.Id, user.Username, user.Email, user.CreatedAt)
        );
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            // Revoke all refresh tokens
            foreach (var token in user.RefreshTokens.Where(rt => !rt.IsRevoked))
            {
                token.IsRevoked = true;
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task StoreRefreshTokenAsync(Guid userId, string token, CancellationToken cancellationToken)
    {
        var refreshTokenExpirationDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");
        
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        user?.RefreshTokens.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
