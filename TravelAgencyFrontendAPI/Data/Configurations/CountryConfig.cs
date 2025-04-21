using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class CountryConfig : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> entity)
        {
            entity.ToTable("T_Countries");

            entity.HasKey(e => e.CountryId);
            entity.Property(e => e.CountryName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Continent).HasMaxLength(50).IsRequired();
        }
    }

}
