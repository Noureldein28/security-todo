namespace SecureTodo.Domain.Entities;

/// <summary>
/// Refresh token entity for JWT token refresh
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
