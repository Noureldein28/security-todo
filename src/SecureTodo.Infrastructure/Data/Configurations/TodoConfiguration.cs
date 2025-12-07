using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureTodo.Domain.Entities;

namespace SecureTodo.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Todo entity
/// </summary>
public class TodoConfiguration : IEntityTypeConfiguration<Todo>
{
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        builder.ToTable("Todos");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.UserId)
            .IsRequired();
        
        builder.Property(t => t.EncryptedContent)
            .IsRequired();
        
        builder.Property(t => t.IV)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(t => t.AuthTag)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(t => t.IntegrityHash)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(t => t.CreatedAt)
            .IsRequired();
        
        builder.Property(t => t.UpdatedAt)
            .IsRequired();
        
        // Indexes for performance
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => new { t.UserId, t.CreatedAt });
        
        // Relationship
        builder.HasOne(t => t.User)
            .WithMany(u => u.Todos)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
