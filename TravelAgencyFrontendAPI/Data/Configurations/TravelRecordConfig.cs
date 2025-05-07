using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class TravelRecordConfig : IEntityTypeConfiguration<TravelRecord>
    {
        public void Configure(EntityTypeBuilder<TravelRecord> builder)
        {
            builder.ToTable("T_TravelRecord");

            builder.HasKey(t => t.TravelRecordId);

            builder.Property(t => t.TotalParticipants).HasDefaultValue(0);
            builder.Property(t => t.TotalOrders).HasDefaultValue(0);
            builder.Property(t => t.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Property(t => t.Note).HasMaxLength(255);
            builder.Property(t => t.CreatedAt).HasDefaultValueSql("GETDATE()").HasColumnType("datetime");
            builder.Property(t => t.UpdatedAt).HasDefaultValueSql("GETDATE()").HasColumnType("datetime");

            builder.HasOne(t => t.GroupTravel)
                   .WithMany(g => g.TravelRecords)
                   .HasForeignKey(t => t.GroupTravelId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_TravelRecord_TotalParticipants", "TotalParticipants >= 0");
                t.HasCheckConstraint("CK_TravelRecord_TotalOrders", "TotalOrders >= 0");
                t.HasCheckConstraint("CK_TravelRecord_TotalAmount", "TotalAmount >= 0.00");
            });
        }
    }
}
