using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
   public class OfficialRestaurantConfig : IEntityTypeConfiguration<OfficialRestaurant>
   {
      public void Configure(EntityTypeBuilder<OfficialRestaurant> entity)
      {
                entity.ToTable("T_Official_Restaurant");

                entity.HasKey(e => e.RestaurantId);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(200).IsRequired(false);
                entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)").IsRequired(false);
                entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)").IsRequired(false);

                entity.HasOne(e => e.Region)
                      .WithMany()
                      .HasForeignKey(e => e.RegionId)
                      .IsRequired(false);

                entity.HasOne(e => e.TravelSupplier)
                      .WithMany()
                      .HasForeignKey(e => e.TravelSupplierId)
                      .IsRequired(false);
      }
   }
}
