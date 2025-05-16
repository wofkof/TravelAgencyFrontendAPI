using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class TransportConfig : IEntityTypeConfiguration<Transport>
    {
        public void Configure(EntityTypeBuilder<Transport> entity)
        {
            entity.ToTable("S_Transport");

            entity.HasKey(e => e.TransportId);
            entity.Property(e => e.TransportMethod).HasMaxLength(50).IsRequired();
        }
    }

}
