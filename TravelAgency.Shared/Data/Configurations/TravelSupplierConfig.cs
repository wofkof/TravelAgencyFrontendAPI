using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class TravelSupplierConfig : IEntityTypeConfiguration<TravelSupplier>
    {
        public void Configure(EntityTypeBuilder<TravelSupplier> entity)
        {
            entity.ToTable("T_TravelSupplier");

            entity.HasKey(e => e.TravelSupplierId);
            entity.Property(e => e.SupplierName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SupplierType).HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(e => e.ContactName).HasMaxLength(50).IsRequired(false);
            entity.Property(e => e.ContactPhone).HasMaxLength(20).IsRequired(false);
            entity.Property(e => e.ContactEmail).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.SupplierNote).HasMaxLength(255).IsRequired(false);
        }
    }

}
