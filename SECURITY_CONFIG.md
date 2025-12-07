# 🔐 Security Configuration Guide

This guide explains how to generate and configure all required security keys for the SecureTodo ASP.NET Core application.

## Required Secrets

The application requires the following secrets to be configured:

1. **JWT Secret Key** (64 bytes recommended)
2. **AES Encryption Key** (32 bytes required)
3. **Database Connection String**
4. **Google OAuth Credentials** (optional)

## Generating Secrets

### Option 1: Using PowerShell (Windows)

#### JWT Secret Key (64 bytes)
```powershell
# Generate a random 64-byte key
$bytes = New-Object byte[] 64
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$jwtKey = [Convert]::ToBase64String($bytes)
Write-Host "JWT Secret Key: $jwtKey"
```

#### AES Encryption Key (32 bytes)
```powershell
# Generate a random 32-byte key
$bytes = New-Object byte[] 32
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$aesKey = [Convert]::ToBase64String($bytes)
Write-Host "AES Key: $aesKey"
```

### Option 2: Using OpenSSL (Linux/Mac/Git Bash on Windows)

#### JWT Secret Key
```bash
openssl rand -base64 64
```

#### AES Encryption Key
```bash
openssl rand -base64 32
```

### Option 3: Using .NET User Secrets (Recommended for Development)

The .NET User Secrets tool stores secrets outside your project directory, preventing accidental commits to source control.

#### Initialize User Secrets
```bash
cd src/SecureTodo.API
dotnet user-secrets init
```

#### Set JWT Secret
```bash
# Generate and set in one command (Linux/Mac/Git Bash)
dotnet user-secrets set "JwtSettings:SecretKey" "$(openssl rand -base64 64)"

# Or set manually
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_GENERATED_KEY_HERE"
```

#### Set AES Encryption Key
```bash
# Generate and set in one command
dotnet user-secrets set "EncryptionSettings:AesKey" "$(openssl rand -base64 32)"

# Or set manually
dotnet user-secrets set "EncryptionSettings:AesKey" "YOUR_GENERATED_KEY_HERE"
```

#### View Current Secrets
```bash
dotnet user-secrets list
```

#### Remove a Secret
```bash
dotnet user-secrets remove "JwtSettings:SecretKey"
```

#### Clear All Secrets
```bash
dotnet user-secrets clear
```

## Configuration Methods

### Method 1: appsettings.Development.json (Not Recommended for Production)

**For Development Only** - Do NOT commit real secrets to source control.

```json
{
  "JwtSettings": {
    "SecretKey": "YOUR_GENERATED_JWT_SECRET_HERE",
    "Issuer": "SecureTodoAPI",
    "Audience": "SecureTodoClient",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "EncryptionSettings": {
    "AesKey": "YOUR_GENERATED_AES_KEY_HERE"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SecureTodoDB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### Method 2: Environment Variables

#### Set Environment Variables (PowerShell)
```powershell
$env:JwtSettings__SecretKey = "YOUR_JWT_SECRET"
$env:EncryptionSettings__AesKey = "YOUR_AES_KEY"
$env:ConnectionStrings__DefaultConnection = "YOUR_CONNECTION_STRING"
```

#### Set Environment Variables (Bash)
```bash
export JwtSettings__SecretKey="YOUR_JWT_SECRET"
export EncryptionSettings__AesKey="YOUR_AES_KEY"
export ConnectionStrings__DefaultConnection="YOUR_CONNECTION_STRING"
```

**Note**: Use double underscores (`__`) to represent nested configuration in environment variables.

### Method 3: Azure Key Vault (Recommended for Production)

#### 1. Create Azure Key Vault
```bash
az keyvault create --name SecureTodoVault --resource-group MyResourceGroup --location eastus
```

#### 2. Add Secrets to Key Vault
```bash
# Add JWT Secret
az keyvault secret set --vault-name SecureTodoVault --name JwtSettings--SecretKey --value "YOUR_JWT_SECRET"

# Add AES Key
az keyvault secret set --vault-name SecureTodoVault --name EncryptionSettings--AesKey --value "YOUR_AES_KEY"

# Add Connection String
az keyvault secret set --vault-name SecureTodoVault --name ConnectionStrings--DefaultConnection --value "YOUR_CONNECTION_STRING"
```

#### 3. Configure Application to Use Key Vault

Add to `Program.cs`:
```csharp
if (builder.Environment.IsProduction())
{
    var keyVaultEndpoint = new Uri(builder.Configuration["KeyVaultEndpoint"]!);
    builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
}
```

Add NuGet package:
```bash
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
dotnet add package Azure.Identity
```

### Method 4: AWS Secrets Manager (Alternative for Production)

#### 1. Install AWS SDK
```bash
dotnet add package AWSSDK.SecretsManager
dotnet add package AWSSDK.Extensions.NETCore.Setup
```

#### 2. Create Secret in AWS
```bash
aws secretsmanager create-secret --name SecureTodo/JwtSecret --secret-string "YOUR_JWT_SECRET"
aws secretsmanager create-secret --name SecureTodo/AesKey --secret-string "YOUR_AES_KEY"
```

## Google OAuth Configuration

### 1. Create Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable Google+ API

### 2. Create OAuth 2.0 Credentials

1. Navigate to **APIs & Services** > **Credentials**
2. Click **Create Credentials** > **OAuth 2.0 Client ID**
3. Configure OAuth consent screen
4. Select **Web application** as application type
5. Add authorized redirect URIs:
   - Development: `http://localhost:5000/api/auth/google-callback`
   - Development HTTPS: `https://localhost:7000/api/auth/google-callback`
   - Production: `https://yourdomain.com/api/auth/google-callback`

### 3. Configure Application

#### Using appsettings.json
```json
{
  "GoogleAuth": {
    "ClientId": "123456789-abcdefghijklmnop.apps.googleusercontent.com",
    "ClientSecret": "GOCSPX-your_client_secret_here",
    "CallbackPath": "/api/auth/google-callback"
  }
}
```

#### Using User Secrets
```bash
dotnet user-secrets set "GoogleAuth:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "GoogleAuth:ClientSecret" "YOUR_CLIENT_SECRET"
```

## Database Connection Strings

### SQL Server LocalDB (Development)
```
Server=(localdb)\mssqllocaldb;Database=SecureTodoDB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true
```

### SQL Server Express
```
Server=localhost\SQLEXPRESS;Database=SecureTodoDB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true
```

### SQL Server with Username/Password
```
Server=localhost;Database=SecureTodoDB;User Id=sa;Password=YourPassword;MultipleActiveResultSets=true;TrustServerCertificate=true
```

### Azure SQL Database
```
Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=SecureTodoDB;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Security Best Practices

### Development
✅ Use .NET User Secrets for local development
✅ Use placeholder values in appsettings.json
✅ Add `appsettings.Development.json` to `.gitignore`
✅ Never commit real secrets to source control
✅ Use different keys for development and production

### Production
✅ Use Azure Key Vault or AWS Secrets Manager
✅ Enable automatic secret rotation
✅ Use Managed Identity for authentication
✅ Implement secret versioning
✅ Monitor secret access with auditing
✅ Use HTTPS only
✅ Enable certificate pinning
✅ Implement rate limiting
✅ Use strong, randomly generated keys
✅ Rotate keys regularly (at least annually)
✅ Revoke compromised keys immediately

## Verifying Configuration

### Check if Secrets are Loaded

Add temporary logging in `Program.cs` (remove in production):

```csharp
// After building the configuration
var jwtKey = builder.Configuration["JwtSettings:SecretKey"];
var aesKey = builder.Configuration["EncryptionSettings:AesKey"];

Console.WriteLine($"JWT Key configured: {!string.IsNullOrEmpty(jwtKey)}");
Console.WriteLine($"AES Key configured: {!string.IsNullOrEmpty(aesKey)}");
```

### Test Key Generation

Run the application and check for encryption errors:

```bash
cd src/SecureTodo.API
dotnet run
```

If keys are not properly configured, you'll see errors like:
- "AES_KEY is not configured"
- "JWT SecretKey not configured"

## Troubleshooting

### Problem: "AES_KEY is not configured"

**Solution**: Ensure the AES key is set and is exactly 32 bytes when base64 decoded.

```bash
# Verify key length
$key = [Convert]::FromBase64String("YOUR_KEY")
$key.Length  # Should be 32
```

### Problem: "JWT SecretKey not configured"

**Solution**: Ensure JWT secret is set in configuration.

```bash
dotnet user-secrets list
```

### Problem: Secrets not loading from User Secrets

**Solution**: Ensure user secrets are initialized for the project:

```bash
cd src/SecureTodo.API
dotnet user-secrets init
dotnet user-secrets list
```

### Problem: Environment variables not working

**Solution**: Restart your terminal/IDE after setting environment variables. Use double underscores for nested paths.

## Quick Setup Script

### PowerShell (Windows)
```powershell
# Navigate to API project
cd src/SecureTodo.API

# Initialize user secrets
dotnet user-secrets init

# Generate and set JWT secret
$jwtBytes = New-Object byte[] 64
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($jwtBytes)
$jwtKey = [Convert]::ToBase64String($jwtBytes)
dotnet user-secrets set "JwtSettings:SecretKey" $jwtKey

# Generate and set AES key
$aesBytes = New-Object byte[] 32
$rng.GetBytes($aesBytes)
$aesKey = [Convert]::ToBase64String($aesBytes)
dotnet user-secrets set "EncryptionSettings:AesKey" $aesKey

# Display keys
Write-Host "Configuration complete!"
Write-Host "JWT Key: $jwtKey"
Write-Host "AES Key: $aesKey"
```

### Bash (Linux/Mac)
```bash
#!/bin/bash

# Navigate to API project
cd src/SecureTodo.API

# Initialize user secrets
dotnet user-secrets init

# Generate and set secrets
dotnet user-secrets set "JwtSettings:SecretKey" "$(openssl rand -base64 64)"
dotnet user-secrets set "EncryptionSettings:AesKey" "$(openssl rand -base64 32)"

# Display confirmation
echo "Configuration complete!"
dotnet user-secrets list
```

## References

- [Safe storage of app secrets in development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault configuration provider](https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Best practices for secrets management](https://learn.microsoft.com/en-us/azure/key-vault/general/best-practices)

---

**Remember**: Never commit secrets to source control! Use proper secret management for all environments.
