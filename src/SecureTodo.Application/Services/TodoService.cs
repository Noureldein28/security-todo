using Microsoft.Extensions.Logging;
using SecureTodo.Application.DTOs;
using SecureTodo.Domain.Entities;
using SecureTodo.Domain.Interfaces;
using SecureTodo.Infrastructure.Security;
using SecureTodo.Shared.Constants;
using SecureTodo.Shared.Exceptions;

namespace SecureTodo.Application.Services;

/// <summary>
/// Todo service implementation with encryption/decryption
/// </summary>
public class TodoService : ITodoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly CryptoService _cryptoService;
    private readonly HashService _hashService;
    private readonly ILogger<TodoService> _logger;

    public TodoService(
        IUnitOfWork unitOfWork,
        CryptoService cryptoService,
        HashService hashService,
        ILogger<TodoService> logger)
    {
        _unitOfWork = unitOfWork;
        _cryptoService = cryptoService;
        _hashService = hashService;
        _logger = logger;
    }

    public async Task<IEnumerable<TodoDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var todos = await _unitOfWork.Todos.GetByUserIdAsync(userId, cancellationToken);
        var result = new List<TodoDto>();

        foreach (var todo in todos)
        {
            result.Add(await DecryptTodoAsync(todo));
        }

        return result;
    }

    public async Task<TodoDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var todo = await _unitOfWork.Todos.GetByIdAndUserIdAsync(id, userId, cancellationToken);
        
        if (todo == null)
        {
            return null;
        }

        return await DecryptTodoAsync(todo);
    }

    public async Task<TodoDto> CreateAsync(CreateTodoDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        // Compute integrity hash
        var integrityHash = _hashService.ComputeSHA256(dto.Content);

        // Encrypt content
        var (encryptedContent, iv, authTag) = _cryptoService.Encrypt(dto.Content);

        // Create todo
        var todo = new Todo
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EncryptedContent = encryptedContent,
            IV = iv,
            AuthTag = authTag,
            IntegrityHash = integrityHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Todos.AddAsync(todo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Todo created by user {UserId}", userId);

        return new TodoDto(todo.Id, dto.Content, false, todo.CreatedAt, todo.UpdatedAt);
    }

    public async Task<TodoDto> UpdateAsync(Guid id, UpdateTodoDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        var todo = await _unitOfWork.Todos.GetByIdAndUserIdAsync(id, userId, cancellationToken);
        
        if (todo == null)
        {
            throw new NotFoundException(ErrorMessages.TodoNotFound);
        }

        // Re-encrypt with new content
        var integrityHash = _hashService.ComputeSHA256(dto.Content);
        var (encryptedContent, iv, authTag) = _cryptoService.Encrypt(dto.Content);

        todo.EncryptedContent = encryptedContent;
        todo.IV = iv;
        todo.AuthTag = authTag;
        todo.IntegrityHash = integrityHash;
        todo.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Todos.UpdateAsync(todo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Todo {TodoId} updated by user {UserId}", id, userId);

        return new TodoDto(todo.Id, dto.Content, false, todo.CreatedAt, todo.UpdatedAt);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var todo = await _unitOfWork.Todos.GetByIdAndUserIdAsync(id, userId, cancellationToken);
        
        if (todo == null)
        {
            throw new NotFoundException(ErrorMessages.TodoNotFound);
        }

        await _unitOfWork.Todos.DeleteAsync(todo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Todo {TodoId} deleted by user {UserId}", id, userId);
    }

    private async Task<TodoDto> DecryptTodoAsync(Todo todo)
    {
        try
        {
            // Decrypt content
            var plaintext = _cryptoService.Decrypt(todo.EncryptedContent, todo.IV, todo.AuthTag);

            // Verify integrity
            var isValid = _hashService.VerifyIntegrity(plaintext, todo.IntegrityHash);

            if (!isValid)
            {
                _logger.LogWarning("Todo integrity check failed for todo {TodoId}", todo.Id);
                return new TodoDto(
                    todo.Id,
                    "[INTEGRITY VIOLATION - Content may have been tampered with]",
                    true,
                    todo.CreatedAt,
                    todo.UpdatedAt
                );
            }

            return new TodoDto(todo.Id, plaintext, false, todo.CreatedAt, todo.UpdatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Decryption failed for todo {TodoId}", todo.Id);
            return new TodoDto(
                todo.Id,
                "[DECRYPTION FAILED - Content is corrupted]",
                true,
                todo.CreatedAt,
                todo.UpdatedAt
            );
        }
    }
}
