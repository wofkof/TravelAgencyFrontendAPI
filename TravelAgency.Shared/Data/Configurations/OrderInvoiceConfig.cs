using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class OrderInvoiceConfig : IEntityTypeConfiguration<OrderInvoice>
    {
        public void Configure(EntityTypeBuilder<OrderInvoice> builder)
        {
            builder.ToTable("T_OrderInvoices");

            builder.HasKey(i => i.InvoiceId);

            builder.Property(i => i.InvoiceNumber).HasMaxLength(10);
            builder.Property(i => i.BuyerName).HasMaxLength(100);
            builder.Property(i => i.InvoiceFileURL).HasMaxLength(255);
            builder.Property(i => i.BuyerUniformNumber).HasMaxLength(8);
            builder.Property(i => i.Note).HasMaxLength(255);

            builder.Property(i => i.CreatedAt).HasDefaultValueSql("GETDATE()");
            builder.Property(i => i.UpdatedAt).HasDefaultValueSql("GETDATE()");

            builder.Property(i => i.TotalAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(i => i.InvoiceType)
                   .HasConversion<string>()
                   .IsRequired()
                   .HasMaxLength(20)
                   .HasDefaultValue(InvoiceType.ElectronicInvoice);

            builder.Property(i => i.InvoiceStatus)
                   .HasConversion<string>()
                   .IsRequired()
                   .HasMaxLength(20)
                   .HasDefaultValue(InvoiceStatus.Pending);

            builder.HasOne(i => i.Order)
                   .WithMany(o => o.OrderInvoices)
                   .HasForeignKey(i => i.OrderId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(i => i.InvoiceNumber).IsUnique();

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_OrderInvoices_TotalAmount", "TotalAmount >= 0.00");
            });
        }
    }
}
