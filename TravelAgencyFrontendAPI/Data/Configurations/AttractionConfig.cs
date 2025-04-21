using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class AttractionConfig : IEntityTypeConfiguration<Attraction>
    {
        public void Configure(EntityTypeBuilder<Attraction> entity)
        {
            entity.ToTable("T_Attraction");

            entity.HasKey(e => e.AttractionId);
            entity.Property(e => e.AttractionName).HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.District)
                  .WithMany()
                  .HasForeignKey(e => e.DistrictId);
        }
    }

}
