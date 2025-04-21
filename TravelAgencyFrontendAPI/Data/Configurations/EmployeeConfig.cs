using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class EmployeeConfig : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> entity)
        {
            entity.ToTable("T_Employee");
            entity.HasKey(e => e.EmployeeId);

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsRequired();
            entity.HasIndex(e => e.Phone).IsUnique();

            entity.Property(e => e.BirthDate).HasColumnType("date").IsRequired(false);

            entity.Property(e => e.HireDate).HasColumnType("date").HasDefaultValueSql("GETDATE()").IsRequired();

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(EmployeeStatus.Active).IsRequired();

            entity.Property(e => e.Note)
                .HasMaxLength(255);

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Employees)
                  .HasForeignKey(e => e.RoleId);
        }
    }
}
