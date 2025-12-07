namespace SecureTodo.Domain.Entities;

/// <summary>
/// Todo entity with encrypted content
/// </summary>
public class Todo
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EncryptedContent { get; set; } = string.Empty;
    public string IV { get; set; } = string.Empty;
    public string AuthTag { get; set; } = string.Empty;
    public string IntegrityHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
