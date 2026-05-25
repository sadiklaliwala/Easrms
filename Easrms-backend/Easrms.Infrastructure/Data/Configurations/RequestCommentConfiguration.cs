using Easrms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Easrms.Infrastructure.Data.Configurations;

public class RequestCommentConfiguration : IEntityTypeConfiguration<RequestComment>
{
    public void Configure(EntityTypeBuilder<RequestComment> builder)
    {
        builder.ToTable("RequestComments");

        builder.HasKey(x => x.CommentId);

        builder.Property(x => x.CommentText)
               .IsRequired()
               .HasMaxLength(1000);
        builder.HasOne(x => x.CommentByUser)
                .WithMany()
                .HasForeignKey(x => x.CommentBy);

        // Store enum as int in database
        builder.Property(x => x.CommentType)
               .IsRequired()
               .HasConversion<int>();

        builder.Property(x => x.IsDeleted)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(x => x.CreatedOn)
               .IsRequired();

        // Request relationship
        builder.HasOne(x => x.ServiceRequest)
               .WithMany(x => x.Comments)
               .HasForeignKey(x => x.RequestId)
               .OnDelete(DeleteBehavior.Restrict);

        // CommentBy relationship
        builder.HasOne(rc => rc.CommentByUser)
                .WithMany(u => u.Comments)
                .HasForeignKey(rc => rc.CommentBy)
                .OnDelete(DeleteBehavior.Restrict);
    }
}
