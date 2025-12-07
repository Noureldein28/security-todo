using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SecureTodo.Infrastructure.Security;

/// <summary>
/// AES-256-GCM encryption and decryption service
/// Provides authenticated encryption matching Node.js implementation
/// </summary>
public class CryptoService
{
    private readonly byte[] _aesKey;
    private const int IVLength = 12; // 96 bits for GCM
    private const int AuthTagLength = 16; // 128 bits

    public CryptoService(IConfiguration configuration)
    {
        var base64Key = configuration["EncryptionSettings:AesKey"];
        if (string.IsNullOrEmpty(base64Key))
        {
            throw new InvalidOperationException("AES_KEY is not configured");
        }

        _aesKey = Convert.FromBase64String(base64Key);
        if (_aesKey.Length != 32)
        {
            throw new InvalidOperationException($"AES key must be 32 bytes (256 bits), got {_aesKey.Length} bytes");
        }
    }

    /// <summary>
    /// Encrypt plaintext using AES-256-GCM
    /// </summary>
    /// <param name="plaintext">Content to encrypt</param>
    /// <returns>Tuple of (encryptedContent, iv, authTag) all base64 encoded</returns>
    public (string encryptedContent, string iv, string authTag) Encrypt(string plaintext)
    {
        try
        {
            // Generate unique random IV
            var iv = new byte[IVLength];
            RandomNumberGenerator.Fill(iv);

            // Create cipher
            using var aesGcm = new AesGcm(_aesKey, AuthTagLength);
            
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var ciphertext = new byte[plaintextBytes.Length];
            var authTag = new byte[AuthTagLength];

            // Encrypt
            aesGcm.Encrypt(iv, plaintextBytes, ciphertext, authTag);

            return (
                Convert.ToBase64String(ciphertext),
                Convert.ToBase64String(iv),
                Convert.ToBase64String(authTag)
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Encryption failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Decrypt ciphertext using AES-256-GCM
    /// </summary>
    /// <param name="encryptedContent">Base64 encoded ciphertext</param>
    /// <param name="ivBase64">Base64 encoded IV</param>
    /// <param name="authTagBase64">Base64 encoded authentication tag</param>
    /// <returns>Decrypted plaintext</returns>
    public string Decrypt(string encryptedContent, string ivBase64, string authTagBase64)
    {
        try
        {
            var ciphertext = Convert.FromBase64String(encryptedContent);
            var iv = Convert.FromBase64String(ivBase64);
            var authTag = Convert.FromBase64String(authTagBase64);

            using var aesGcm = new AesGcm(_aesKey, AuthTagLength);
            
            var plaintext = new byte[ciphertext.Length];

            // Decrypt and verify auth tag
            aesGcm.Decrypt(iv, ciphertext, authTag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Decryption failed: {ex.Message}", ex);
        }
    }
}
