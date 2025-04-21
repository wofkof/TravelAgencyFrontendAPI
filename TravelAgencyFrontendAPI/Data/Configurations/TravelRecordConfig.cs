using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class TravelRecordConfig : IEntityTypeConfiguration<TravelRecord>
    {
        public void Configure(EntityTypeBuilder<TravelRecord> entity)
        {
            entity.ToTable("T_TravelRecord");
            entity.HasKey(e => e.TravelRecordId);

            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.TotalParticipants).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Order)
                  .WithMany()
                  .HasForeignKey(e => e.OrderId);
        }
    }

}
