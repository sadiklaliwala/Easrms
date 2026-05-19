using Easrms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Easrms.Infrastructure.Data.Configurations;

public class RequestStatusHistoryConfiguration : IEntityTypeConfiguration<RequestStatusHistory>
{
    public void Configure(EntityTypeBuilder<RequestStatusHistory> builder)
    {
        builder.ToTable("RequestStatusHistories");

        builder.HasKey(x => x.HistoryId);

        builder.Property(x => x.OldStatus)
               .HasMaxLength(30)
               .IsRequired(false);

        builder.Property(x => x.NewStatus)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(x => x.Remarks)
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(x => x.ChangedOn)
               .IsRequired();

        // Request relationship
        builder.HasOne(rsh => rsh.Request)
                .WithMany(sr => sr.StatusHistories)
                .HasForeignKey(rsh => rsh.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

        // ChangedBy relationship
        builder.HasOne(rsh => rsh.ChangedByUser)
                .WithMany(u => u.StatusHistories)
                .HasForeignKey(rsh => rsh.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);
    }
}

