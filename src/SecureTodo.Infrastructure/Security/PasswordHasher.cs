using BCrypt.Net;

namespace SecureTodo.Infrastructure.Security;

/// <summary>
/// BCrypt password hashing service
/// Uses 12 rounds as specified in requirements
/// </summary>
public class PasswordHasher
{
    private const int WorkFactor = 12;

    /// <summary>
    /// Hash a password using BCrypt
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>BCrypt hashed password</returns>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// Verify a password against a hash
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hash">BCrypt hash to compare against</param>
    /// <returns>True if password matches hash</returns>
    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
