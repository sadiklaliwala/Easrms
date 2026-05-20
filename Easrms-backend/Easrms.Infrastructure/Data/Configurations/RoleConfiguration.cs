using Easrms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Easrms.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => x.RoleId);

        builder.Property(x => x.RoleName)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasData(
            new Role { RoleId = Guid.Parse("90a5eb56-0ddc-4187-9f32-8870f8fc7043"), RoleName = "Admin" },
            new Role { RoleId = Guid.Parse("90a5eb56-0ddc-4187-9f32-8870f8fc7044"), RoleName = "Manager" },
            new Role { RoleId = Guid.Parse("90a5eb56-0ddc-4187-9f32-8870f8fc7045"), RoleName = "Employee" },
            new Role { RoleId = Guid.Parse("90a5eb56-0ddc-4187-9f32-8870f8fc7046"), RoleName = "Support" }
        );
    }
}
