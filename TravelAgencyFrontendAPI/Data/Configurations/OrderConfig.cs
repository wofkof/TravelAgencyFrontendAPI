using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("T_Order");

            builder.HasKey(o => o.OrderId);

            builder.Property(o => o.PaymentMethod)
                   .HasConversion<string>()
                   .HasDefaultValue(PaymentMethod.Other)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(o => o.Status)
                   .HasConversion<string>()
                   .IsRequired()
                   .HasMaxLength(20)
                   .HasDefaultValue(OrderStatus.Pending);

            builder.Property(o => o.CreatedAt)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(o => o.PaymentDate)
                   .HasColumnType("datetime");

            builder.Property(o => o.TotalAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(o => o.InvoiceOption)
                   .HasDefaultValue(InvoiceOption.Personal)
                   .HasConversion<string>()
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(o => o.InvoiceAddBillingAddr)
                   .HasDefaultValue(false);

            builder.Property(o => o.InvoiceDeliveryEmail).HasMaxLength(255);
            builder.Property(o => o.InvoiceUniformNumber).HasMaxLength(8);
            builder.Property(o => o.InvoiceTitle).HasMaxLength(100);
            builder.Property(o => o.InvoiceBillingAddress).HasMaxLength(255);
            builder.Property(o => o.Note).HasMaxLength(255);

            builder.HasOne(o => o.Member)
                   .WithMany(m => m.Orders)
                   .HasForeignKey(o => o.MemberId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
