using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> entity)
        {
            entity.ToTable("T_Order");
            entity.HasKey(e => e.OrderId);

            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.ParticipantsCount).IsRequired();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.PaymentDate).HasColumnType("datetime").IsRequired(false);
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(e => e.Category)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(e => e.PaymentMethod)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(e => e.Note).HasMaxLength(255).IsRequired(false);

            entity.HasOne(e => e.Member)
                  .WithMany(m => m.Orders)
                  .HasForeignKey(e => e.MemberId);
        }
    }

}
