using SecureTodo.Domain.Entities;

namespace SecureTodo.Domain.Interfaces;

/// <summary>
/// User-specific repository operations
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
