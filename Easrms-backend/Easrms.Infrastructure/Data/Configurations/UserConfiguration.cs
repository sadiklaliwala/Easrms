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
               .IsRequired();

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

        //builder.HasData(
        //    new User
        //    {
        //        UserId = Guid.Parse("7c618c94-6873-47e8-84f8-592d9a018374"),
        //        FullName = "System Admin",
        //        Email = "admin@easrms.com",
        //        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
        //        RoleId = Guid.Parse("90a5eb56-0ddc-4187-9f32-8870f8fc7043"),
        //        ManagerId = null,
        //        IsActive = true,
        //        CreatedOn = new DateTime(2026, 1, 1)
        //    }
        //);
    }
}

