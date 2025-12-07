using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTodo.Application.DTOs;
using SecureTodo.Application.Services;
using SecureTodo.Shared.Results;
using System.Security.Claims;

namespace SecureTodo.API.Controllers;

/// <summary>
/// Authentication controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(Result<LoginResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RegisterAsync(dto, cancellationToken);
            
            SetTokenCookie(response.AccessToken);
            
            return CreatedAtAction(nameof(Register), Result<LoginResponseDto>.SuccessResult(
                response, "User registered successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed");
            return BadRequest(Result<string>.FailureResult("Registration failed", ex.Message));
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(dto, cancellationToken);
            
            SetTokenCookie(response.AccessToken);
            
            return Ok(Result<LoginResponseDto>.SuccessResult(response, "Login successful"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Login failed for {Email}", dto.Email);
            return Unauthorized(Result<string>.FailureResult("Login failed", ex.Message));
        }
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(Result<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(dto, cancellationToken);
            
            SetTokenCookie(response.AccessToken);
            
            return Ok(Result<LoginResponseDto>.SuccessResult(response, "Token refreshed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token refresh failed");
            return Unauthorized(Result<string>.FailureResult("Token refresh failed", ex.Message));
        }
    }

    /// <summary>
    /// Logout user
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            await _authService.LogoutAsync(userId, cancellationToken);
            
            Response.Cookies.Delete("token");
            
            return Ok(Result<string>.SuccessResult("Logged out successfully", "Logout successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return StatusCode(500, Result<string>.FailureResult("Logout failed", ex.Message));
        }
    }

    /// <summary>
    /// Initiate Google OAuth login
    /// </summary>
    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        // This would redirect to Google OAuth
        // For now, return a placeholder
        return Ok(new { message = "Redirect to Google OAuth", callbackUrl = "/api/auth/google-callback" });
    }

    /// <summary>
    /// Google OAuth callback
    /// </summary>
    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, CancellationToken cancellationToken)
    {
        try
        {
            // This is a simplified implementation
            // In production, you would:
            // 1. Exchange code for tokens with Google
            // 2. Get user info from Google
            // 3. Call AuthService.GoogleLoginAsync
            
            return Ok(new { message = "Google OAuth callback - implement token exchange" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google OAuth callback failed");
            return BadRequest(Result<string>.FailureResult("Google login failed", ex.Message));
        }
    }

    private void SetTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        };
        
        Response.Cookies.Append("token", token, cookieOptions);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}
