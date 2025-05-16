using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class PickupInformationConfig : IEntityTypeConfiguration<PickupInformation>
    {
        public void Configure(EntityTypeBuilder<PickupInformation> entity)
        {
            entity.ToTable("T_PickupInformation");
            entity.HasKey(e => e.PickupInfoId);

            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(10);
            entity.Property(e => e.DetailedAddress).HasMaxLength(255);
        }
    }
}
