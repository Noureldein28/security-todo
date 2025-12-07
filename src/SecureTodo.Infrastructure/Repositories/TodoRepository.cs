using Microsoft.EntityFrameworkCore;
using SecureTodo.Domain.Entities;
using SecureTodo.Domain.Interfaces;
using SecureTodo.Infrastructure.Data;

namespace SecureTodo.Infrastructure.Repositories;

/// <summary>
/// Todo repository implementation
/// </summary>
public class TodoRepository : Repository<Todo>, ITodoRepository
{
    public TodoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Todo>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Todo?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);
    }
}
