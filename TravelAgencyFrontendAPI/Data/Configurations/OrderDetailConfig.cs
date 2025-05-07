using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class OrderDetailConfig : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            builder.ToTable("T_OrderDetail");

            builder.HasKey(od => od.OrderDetailId);

            builder.Property(od => od.Category)
                   .HasConversion<string>()
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(od => od.Quantity)
                   .HasDefaultValue(1);

            builder.Property(od => od.Price)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(od => od.TotalAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(od => od.CreatedAt)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(od => od.UpdatedAt)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(od => od.StartDate)
                   .HasColumnType("datetime");

            builder.Property(od => od.Description).HasMaxLength(255);
            builder.Property(od => od.Note).HasMaxLength(255);

            builder.HasOne(od => od.Order)
                   .WithMany(o => o.OrderDetails)
                   .HasForeignKey(od => od.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_OrderDetail_Quantity", "Quantity > 0");
                t.HasCheckConstraint("CK_OrderDetail_Price", "Price >= 0.00");
            });
        }
    }
}
