using Easrms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Easrms.Infrastructure.Data.Configurations;

public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
{
    public void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {
        builder.ToTable("ServiceRequests");

        builder.HasKey(x => x.RequestId);

        builder.Property(x => x.RequestNumber)
               .IsRequired()
               .HasMaxLength(20);

        builder.HasIndex(x => x.RequestNumber)
               .IsUnique();

        builder.Property(x => x.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.Description)
               .IsRequired()
               .HasMaxLength(1000);

        builder.Property(x => x.Priority)
               //.IsRequired()
               .HasMaxLength(20);

        builder.Property(x => x.Status)
               //.IsRequired()
               .HasMaxLength(30);

        builder.Property(x => x.RejectionReason)
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(x => x.ResolvedOn)
               .IsRequired(false);

        builder.Property(x => x.ClosedOn)
               .IsRequired(false);

        // CR-001 added
        builder.Property(x => x.DueDate)
            .IsRequired(false);

        builder.Property(x => x.IsEscalated)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.EscalatedOn)
            .IsRequired(false);

        builder.Property(x => x.EscalationReason)
            .IsRequired(false)
            .HasMaxLength(500);

        // AttachmentUrl mapping
        builder.Property(x => x.AttachmentUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        // FK → Users (Escalator)
        builder.HasOne(x => x.Escalator)
            .WithMany()
            .HasForeignKey(x => x.EscalatedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Employee relationship
        builder.HasOne(x => x.Employee)
               .WithMany(x => x.CreatedRequests)
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        // Category relationship
        builder.HasOne(x => x.Category)
               .WithMany(x => x.ServiceRequests)
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        // AssignedTo relationship
        builder.HasOne(x => x.AssignedUser)
               .WithMany(x => x.AssignedRequests)
               .HasForeignKey(x => x.AssignedTo)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        // ClosedBy relationship
        builder.HasOne(x => x.ClosedByUser)
               .WithMany(x => x.ClosedRequests)
               .HasForeignKey(x => x.ClosedBy)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);
    }
}