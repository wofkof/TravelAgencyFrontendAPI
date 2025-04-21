using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class OfficialTravelConfig : IEntityTypeConfiguration<OfficialTravel>
    {
        public void Configure(EntityTypeBuilder<OfficialTravel> entity)
        {
            entity.ToTable("T_OfficialTravel");

            entity.HasKey(o => o.OfficialTravelId);

            entity.Property(o => o.Category)
                  .HasConversion<string>()
                  .HasMaxLength(30)
                  .IsRequired();

            entity.Property(o => o.Title).HasMaxLength(100).IsRequired();
            entity.Property(o => o.AvailableFrom).HasColumnType("datetime").IsRequired(false);
            entity.Property(o => o.AvailableUntil).HasColumnType("datetime").IsRequired(false);
            entity.Property(o => o.Description).HasMaxLength(500).IsRequired(false);
            entity.Property(o => o.TotalTravelCount).IsRequired(false);
            entity.Property(o => o.TotalDepartureCount).IsRequired(false);
            entity.Property(o => o.Days).IsRequired(false);
            entity.Property(o => o.CoverPath).HasMaxLength(255).IsRequired(false);
            entity.Property(o => o.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()").IsRequired(false);
            entity.Property(o => o.UpdatedAt).HasColumnType("datetime").IsRequired(false);

            entity.Property(o => o.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired(false);

            entity.HasOne(o => o.CreatedByEmployee)
                  .WithMany()
                  .HasForeignKey(o => o.CreatedByEmployeeId);

            entity.HasOne(o => o.Region)
                  .WithMany()
                  .HasForeignKey(o => o.RegionId);
        }
    }

}
