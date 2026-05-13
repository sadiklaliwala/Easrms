using Easrms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Easrms.Infrastructure.Data.Configurations;

public class RequestCategoryConfiguration : IEntityTypeConfiguration<RequestCategory>
{
    public void Configure(EntityTypeBuilder<RequestCategory> builder)
    {
        builder.ToTable("RequestCategory");

        builder.HasKey(x => x.CategoryId);

        builder.Property(x => x.CategoryName)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasIndex(x => x.CategoryName)
               .IsUnique();

        builder.Property(x => x.IsApprovalRequired)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedOn)
               .IsRequired();
    }
}