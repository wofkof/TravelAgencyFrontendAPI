using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class OfficialTravelScheduleConfig : IEntityTypeConfiguration<OfficialTravelSchedule>
    {
        public void Configure(EntityTypeBuilder<OfficialTravelSchedule> entity)
        {
            entity.ToTable("T_OfficialTravelSchedule");

            entity.HasKey(e => e.OfficialTravelScheduleId);
            entity.Property(e => e.Day).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.Breakfast).HasMaxLength(20).IsRequired(false);
            entity.Property(e => e.Lunch).HasMaxLength(20).IsRequired(false);
            entity.Property(e => e.Dinner).HasMaxLength(20).IsRequired(false);
            entity.Property(e => e.Hotel).HasMaxLength(20).IsRequired(false);
            entity.Property(e => e.Attraction1).IsRequired(false);
            entity.Property(e => e.Attraction2).IsRequired(false);
            entity.Property(e => e.Attraction3).IsRequired(false);
            entity.Property(e => e.Attraction4).IsRequired(false);
            entity.Property(e => e.Attraction5).IsRequired(false);
            entity.Property(e => e.Note1).HasMaxLength(50).IsRequired(false);
            entity.Property(e => e.Note2).HasMaxLength(50).IsRequired(false);

            entity.HasOne(e => e.OfficialTravelDetail)
                  .WithMany(o => o.officialTravelSchedules)
                  .HasForeignKey(e => e.OfficialTravelDetailId);
        }
    }

}
