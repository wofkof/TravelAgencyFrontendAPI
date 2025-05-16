using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class PaymentConfig : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> entity)
        {
            entity.ToTable("T_Payment");
            entity.HasKey(e => e.PaymentId);

            entity.Property(e => e.PaymentDeadline).HasColumnType("date");
            entity.Property(e => e.PaymentMethod)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.PaymentMethod).HasConversion<string>().HasMaxLength(20).IsRequired();

            entity.HasOne(e => e.DocumentOrderDetails)
                .WithMany()
                .HasForeignKey(e => e.DocumentOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
