namespace SecureTodo.Shared.Constants;

/// <summary>
/// Centralized error messages
/// </summary>
public static class ErrorMessages
{
    // Authentication errors
    public const string UserAlreadyExists = "User with this email already exists";
    public const string InvalidCredentials = "Invalid email or password";
    public const string UserNotFound = "User not found";
    public const string InvalidToken = "Invalid or expired token";
    public const string GoogleUserNoPassword = "This account uses Google Sign-In. Please login with Google.";
    
    // Todo errors
    public const string TodoNotFound = "Todo not found";
    public const string TodoAccessDenied = "You don't have permission to access this todo";
    
    // Validation errors
    public const string InvalidEmail = "Please enter a valid email address";
    public const string PasswordTooShort = "Password must be at least 8 characters";
    public const string UsernameTooShort = "Username must be at least 3 characters";
    public const string ContentRequired = "Content is required";
    public const string ContentTooLong = "Content must not exceed 1000 characters";
    
    // General errors
    public const string InternalServerError = "An error occurred processing your request";
    public const string UnauthorizedAccess = "Unauthorized access";
}
