using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class CommentConfig : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> entity)
        {
            entity.ToTable("T_Comment");

            entity.HasKey(e => e.CommentId);

            entity.Property(e => e.TravelType)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.TravelId)
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

            entity.HasOne(e => e.Member)
                  .WithMany()
                  .HasForeignKey(e => e.MemberId);
        }
    }

}
