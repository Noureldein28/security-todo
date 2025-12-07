using System.Security.Cryptography;
using System.Text;

namespace SecureTodo.Infrastructure.Security;

/// <summary>
/// SHA-256 hashing service for integrity verification
/// </summary>
public class HashService
{
    /// <summary>
    /// Compute SHA-256 hash of content
    /// </summary>
    /// <param name="content">Content to hash</param>
    /// <returns>Hex-encoded SHA-256 hash</returns>
    public string ComputeSHA256(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Verify integrity by comparing hashes
    /// </summary>
    /// <param name="content">Current content</param>
    /// <param name="storedHash">Previously computed hash</param>
    /// <returns>True if hashes match</returns>
    public bool VerifyIntegrity(string content, string storedHash)
    {
        var currentHash = ComputeSHA256(content);
        return string.Equals(currentHash, storedHash, StringComparison.OrdinalIgnoreCase);
    }
}
