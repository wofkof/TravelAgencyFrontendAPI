using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class CityConfig : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> entity)
        {
            entity.ToTable("S_City");

            entity.HasKey(e => e.CityId);
            entity.Property(e => e.CityName).HasMaxLength(50).IsRequired();
        }
    }

}
