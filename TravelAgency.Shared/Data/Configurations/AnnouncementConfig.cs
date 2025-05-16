using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class AnnouncementConfig : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> entity)
        {
            entity.ToTable("T_Announcement");

            entity.HasKey(a => a.AnnouncementId);
            entity.Property(a => a.Title).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Content).HasMaxLength(500).IsRequired();
            entity.Property(a => a.SentAt).HasColumnType("datetime").IsRequired();

            entity.Property(a => a.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(AnnouncementStatus.Published);

            entity.HasOne(a => a.Employee)
                  .WithMany()
                  .HasForeignKey(a => a.EmployeeId);
        }
    }

}
