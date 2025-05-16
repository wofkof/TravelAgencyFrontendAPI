using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class RestaurantConfig : IEntityTypeConfiguration<Restaurant>
    {
        public void Configure(EntityTypeBuilder<Restaurant> entity)
        {
            entity.ToTable("T_Restaurant");

            entity.HasKey(e => e.RestaurantId);
            entity.Property(e => e.RestaurantName).HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.District)
                  .WithMany()
                  .HasForeignKey(e => e.DistrictId);
        }
    }

}
