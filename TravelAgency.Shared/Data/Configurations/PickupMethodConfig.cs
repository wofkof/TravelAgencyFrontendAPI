using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class PickupMethodConfig : IEntityTypeConfiguration<PickupMethod>
    {
        public void Configure(EntityTypeBuilder<PickupMethod> entity)
        {
            entity.ToTable("T_PickupMethod");
            entity.HasKey(e => e.PickupMethodId);

            entity.Property(e => e.PickupMethodName)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        }
    }
}
