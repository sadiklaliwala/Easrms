using Easrms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Easrms.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.FullName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Email)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(x => x.PasswordHash)
               .IsRequired(false);

        builder.Property(x => x.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedOn)
               .IsRequired();

        builder.Property(x => x.RefreshToken)
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(x => x.RefreshTokenExpiryOn)
               .IsRequired(false);

        // NEW: OTP and profile fields
        builder.Property(u => u.OtpCode).HasMaxLength(500).IsRequired(false);
        builder.Property(u => u.OtpExpiryOn).IsRequired(false);
        builder.Property(u => u.ProfilePhotoUrl).HasMaxLength(500).IsRequired(false);

        // Soft-delete mapping
        builder.Property(u => u.IsDeleted)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(u => u.DeletedOn)
               .IsRequired(false);

        // Role relationship
        builder.HasOne(x => x.Role)
               .WithMany(x => x.Users)
               .HasForeignKey(x => x.RoleId)
               .OnDelete(DeleteBehavior.Restrict);

        // Self reference Manager
        builder.HasOne(u => u.Manager)
                .WithMany(u => u.Employees)
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
    }
}

