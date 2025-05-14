using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class RoleConfig : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> entity)
        {
            entity.ToTable("T_Role");
            entity.HasKey(r => r.RoleId);
            entity.Property(r => r.RoleName).HasMaxLength(50).IsRequired();

        }
    }
}
