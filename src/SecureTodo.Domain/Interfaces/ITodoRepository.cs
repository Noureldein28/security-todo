using SecureTodo.Domain.Entities;

namespace SecureTodo.Domain.Interfaces;

/// <summary>
/// Todo-specific repository operations
/// </summary>
public interface ITodoRepository : IRepository<Todo>
{
    Task<IEnumerable<Todo>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Todo?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
