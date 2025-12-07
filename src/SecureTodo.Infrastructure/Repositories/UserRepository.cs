using Microsoft.EntityFrameworkCore;
using SecureTodo.Domain.Entities;
using SecureTodo.Domain.Interfaces;
using SecureTodo.Infrastructure.Data;

namespace SecureTodo.Infrastructure.Repositories;

/// <summary>
/// User repository implementation
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.GoogleId == googleId, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }
}
