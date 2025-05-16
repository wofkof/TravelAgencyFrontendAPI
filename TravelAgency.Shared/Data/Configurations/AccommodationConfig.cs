using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
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
