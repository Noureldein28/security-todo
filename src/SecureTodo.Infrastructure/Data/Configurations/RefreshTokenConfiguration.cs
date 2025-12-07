using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureTodo.Domain.Entities;

namespace SecureTodo.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for RefreshToken entity
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        
        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.UserId)
            .IsRequired();
        
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.HasIndex(rt => rt.Token)
            .IsUnique();
        
        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();
        
        builder.Property(rt => rt.CreatedAt)
            .IsRequired();
        
        builder.Property(rt => rt.IsRevoked)
            .IsRequired();
        
        // Indexes
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => new { rt.UserId, rt.IsRevoked });
        
        // Relationship
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
