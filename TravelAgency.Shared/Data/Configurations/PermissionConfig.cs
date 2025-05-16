using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class PermissionConfig : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> entity)
        {
            entity.ToTable("T_Permission");

            entity.HasKey(e => e.PermissionId);
            entity.Property(e => e.PermissionName).HasMaxLength(50).IsRequired();
        }
    }

}
