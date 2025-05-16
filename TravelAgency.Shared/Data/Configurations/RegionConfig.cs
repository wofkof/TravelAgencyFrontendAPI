using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class RegionConfig : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> entity)
        {
            entity.ToTable("T_Region");

            entity.HasKey(r => r.RegionId);
            entity.Property(r => r.Country).HasMaxLength(100).IsRequired();
            entity.Property(r => r.Name).HasMaxLength(100).IsRequired();
        }
    }

}
