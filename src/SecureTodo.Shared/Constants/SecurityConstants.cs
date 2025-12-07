namespace SecureTodo.Shared.Constants;

/// <summary>
/// Security-related constants
/// </summary>
public static class SecurityConstants
{
    public const string AesKeyConfigName = "EncryptionSettings:AesKey";
    public const string JwtSecretConfigName = "JwtSettings:SecretKey";
    public const string JwtIssuerConfigName = "JwtSettings:Issuer";
    public const string JwtAudienceConfigName = "JwtSettings:Audience";
    
    public const int AesKeyLength = 32; // 256 bits
    public const int AesIVLength = 12; // 96 bits for GCM
    public const int AesAuthTagLength = 16; // 128 bits
    
    public const int BcryptWorkFactor = 12;
    
    public const int AccessTokenExpirationMinutes = 60;
    public const int RefreshTokenExpirationDays = 7;
}
