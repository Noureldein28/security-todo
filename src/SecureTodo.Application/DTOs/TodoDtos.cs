namespace SecureTodo.Application.DTOs;

/// <summary>
/// Todo-related DTOs
/// </summary>

public record TodoDto(
    Guid Id,
    string Content,
    bool Tampered,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateTodoDto(
    string Content
);

public record UpdateTodoDto(
    string Content
);

public record TodoResponseDto(
    string Message,
    TodoDto Todo
);
