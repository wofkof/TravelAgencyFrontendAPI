using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class AccommodationConfig : IEntityTypeConfiguration<Accommodation>
    {
        public void Configure(EntityTypeBuilder<Accommodation> entity)
        {
            entity.ToTable("T_Accommodation");

            entity.HasKey(e => e.AccommodationId);
            entity.Property(e => e.AccommodationName).HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.District)
                  .WithMany()
                  .HasForeignKey(e => e.DistrictId);
        }
    }

}
