namespace SecureTodo.Shared.Exceptions;

/// <summary>
/// Base exception for business logic errors
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
    public BusinessException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception for resource not found errors
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Exception for validation errors
/// </summary>
public class ValidationException : Exception
{
    public List<string> Errors { get; }
    
    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }
    
    public ValidationException(List<string> errors) : base("Validation failed")
    {
        Errors = errors;
    }
}

/// <summary>
/// Exception for unauthorized access
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
