using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class CallLogConfig : IEntityTypeConfiguration<CallLog>
    {
        public void Configure(EntityTypeBuilder<CallLog> entity)
        {
            entity.ToTable("T_CallLog");

            entity.HasKey(c => c.CallId);

            entity.Property(c => c.CallerType).HasMaxLength(20).HasConversion<string>().IsRequired();
            entity.Property(c => c.CallerId).IsRequired();
            entity.Property(c => c.ReceiverType).HasMaxLength(20).HasConversion<string>().IsRequired();
            entity.Property(c => c.ReceiverId).IsRequired();
            entity.Property(c => c.CallType).HasMaxLength(20).HasConversion<string>().IsRequired();
            entity.Property(c => c.Status).HasMaxLength(20).HasConversion<string>().IsRequired();

            entity.Property(c => c.StartTime).HasColumnType("datetime").IsRequired();
            entity.Property(c => c.EndTime).HasColumnType("datetime").IsRequired(false);
            entity.Property(c => c.DurationInSeconds).IsRequired(false);

            entity.HasOne(c => c.ChatRoom)
                  .WithMany()
                  .HasForeignKey(c => c.ChatRoomId);
        }
    }

}
