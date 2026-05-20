using Easrms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Easrms.Infrastructure.Data.Configurations
{
    public class RequestEscalationHistoryConfiguration : IEntityTypeConfiguration<RequestEscalationHistory>
    {
        public void Configure(EntityTypeBuilder<RequestEscalationHistory> builder)
        {
            builder.ToTable("RequestEscalationHistory");

            builder.HasKey(x => x.EscalationId);

            builder.Property(x => x.EscalationId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.EscalationReason)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.EscalatedOn)
                .IsRequired();

            builder.Property(x => x.CreatedOn)
                .IsRequired();

            // FK → ServiceRequest
            builder.HasOne(x => x.Request)
                .WithMany(r => r.EscalationHistories)
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK → Users
            builder.HasOne(x => x.EscalatedByUser)
                .WithMany()
                .HasForeignKey(x => x.EscalatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}