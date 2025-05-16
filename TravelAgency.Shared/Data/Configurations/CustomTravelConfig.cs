using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class CustomTravelConfig : IEntityTypeConfiguration<CustomTravel>
    {
        public void Configure(EntityTypeBuilder<CustomTravel> entity)
        {
            entity.ToTable("T_CustomTravel");
            entity.HasKey(e => e.CustomTravelId);

            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").IsRequired(false);
            entity.Property(e => e.DepartureDate).HasColumnType("datetime").IsRequired(false);
            entity.Property(e => e.EndDate).HasColumnType("datetime").IsRequired(false);
            entity.Property(e => e.Days);
            entity.Property(e => e.People);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(CustomTravelStatus.Pending);

            entity.Property(e => e.Note).HasMaxLength(255).IsRequired(false);

            entity.HasOne(e => e.Member)
                  .WithMany()
                  .HasForeignKey(e => e.MemberId);

            entity.HasOne(e => e.ReviewEmployee)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewEmployeeId);
        }
    }

}
