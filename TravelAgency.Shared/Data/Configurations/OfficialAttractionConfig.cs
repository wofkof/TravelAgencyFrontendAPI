using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class OfficialAttractionConfig : IEntityTypeConfiguration<OfficialAttraction>
    {
        public void Configure(EntityTypeBuilder<OfficialAttraction> entity)
        {
            entity.ToTable("T_Official_Attraction");

            entity.HasKey(e => e.AttractionId);
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
