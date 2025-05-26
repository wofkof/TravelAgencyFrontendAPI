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

            entity.Property(e => e.PaymentId)
                  .HasColumnName("payment_id");

            entity.Property(e => e.OrderFormId)
                  .HasColumnName("order_form_id");

            entity.HasOne(e => e.OrderForm)
                  .WithMany()
                  .HasForeignKey(e => e.OrderFormId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.DocumentMenuId)
                  .HasColumnName("document_menu_id");

            entity.HasOne(e => e.DocumentMenu)
                  .WithMany()
                  .HasForeignKey(e => e.DocumentMenuId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.PaymentMethod)
                  .HasColumnName("payment_method")
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();
        }
    }
}
