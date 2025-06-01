using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class CommentConfig : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> entity)
        {
            entity.ToTable("T_Comment");

            entity.HasKey(e => e.CommentId);

            entity.Property(e => e.Category)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.OrderDetailId)
                  .IsRequired();

            entity.Property(e => e.Rating)
                  .IsRequired();

            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(CommentStatus.Visible);

            entity.Property(e => e.Content)
                  .HasMaxLength(500)
                  .IsRequired(false);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime")
                  .HasDefaultValueSql("GETDATE()");
            
            entity.HasCheckConstraint("CK_Comment_Rating", "[Rating] BETWEEN 1 AND 5");

            entity.HasIndex(e => new { e.MemberId, e.OrderDetailId }).IsUnique();

            entity.HasOne(e => e.Member)
                    .WithMany(m => m.Comments)
                    .HasForeignKey(e => e.MemberId)
                    .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.OrderDetail)
                      .WithMany(od => od.Comments)
                      .HasForeignKey(e => e.OrderDetailId)
                      .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
