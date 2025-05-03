using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
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

            entity.HasCheckConstraint("CK_PaymentMethod", "payment_method IN ('信用卡', '電子支付')");

            entity.HasOne<DocumentOrderDetails>()
                .WithMany()
                .HasForeignKey(e => e.DocumentOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
