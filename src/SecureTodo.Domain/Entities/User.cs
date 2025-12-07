namespace SecureTodo.Domain.Entities;

/// <summary>
/// User entity representing authentication information
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? GoogleId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Todo> Todos { get; set; } = new List<Todo>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
