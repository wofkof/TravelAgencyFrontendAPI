using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class DistrictConfig : IEntityTypeConfiguration<District>
    {
        public void Configure(EntityTypeBuilder<District> entity)
        {
            entity.ToTable("S_District");

            entity.HasKey(e => e.DistrictId);
            entity.Property(e => e.DistrictName).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.City)
                  .WithMany()
                  .HasForeignKey(e => e.CityId);
        }
    }

}
