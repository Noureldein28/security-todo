using SecureTodo.Application.DTOs;

namespace SecureTodo.Application.Services;

/// <summary>
/// Todo service interface
/// </summary>
public interface ITodoService
{
    Task<IEnumerable<TodoDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TodoDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<TodoDto> CreateAsync(CreateTodoDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<TodoDto> UpdateAsync(Guid id, UpdateTodoDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
