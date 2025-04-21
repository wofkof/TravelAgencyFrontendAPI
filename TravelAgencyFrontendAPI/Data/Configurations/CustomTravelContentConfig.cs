using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class CustomTravelContentConfig : IEntityTypeConfiguration<CustomTravelContent>
    {
        public void Configure(EntityTypeBuilder<CustomTravelContent> entity)
        {
            entity.ToTable("T_CustomTravelContent");
            entity.HasKey(e => e.ContentId);

            entity.Property(e => e.Day).IsRequired();
            entity.Property(e => e.Time).HasMaxLength(20).IsRequired();
            entity.Property(e => e.AccommodationName).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.Category)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.HasOne(e => e.CustomTravel)
                  .WithMany()
                  .HasForeignKey(e => e.CustomTravelId);
        }
    }

}
