using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class CompletedOrderDetailConfig : IEntityTypeConfiguration<CompletedOrderDetail>
    {
        public void Configure(EntityTypeBuilder<CompletedOrderDetail> entity)
        {
            entity.ToTable("T_CompletedOrderDetail");

            entity.HasKey(e => e.CompletedOrderDetailId);

            entity.Property(e => e.CompletedOrderDetailId)
                  .HasColumnName("completed_order_detail_id");

            entity.Property(e => e.DocumentMenuId)
                  .HasColumnName("document_menu_id");

            entity.HasOne(e => e.DocumentMenu)
                  .WithMany()
                  .HasForeignKey(e => e.DocumentMenuId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.OrderFormId)
                  .HasColumnName("order_form_id");

            entity.HasOne(e => e.OrderForm)
                  .WithMany()
                  .HasForeignKey(e => e.OrderFormId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
