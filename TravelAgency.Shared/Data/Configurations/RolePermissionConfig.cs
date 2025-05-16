using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class RolePermissionConfig : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> entity)
        {
            entity.ToTable("M_RolePermission");

            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()").IsRequired();

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(e => e.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
