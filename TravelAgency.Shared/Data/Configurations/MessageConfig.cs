using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class MessageConfig : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> entity)
        {
            entity.ToTable("T_Message");

            entity.HasKey(m => m.MessageId);

            entity.Property(m => m.SenderType).HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(m => m.SenderId).IsRequired();
            entity.Property(m => m.MessageType).HasMaxLength(20).HasConversion<string>().IsRequired();
            entity.Property(m => m.Content).IsRequired();
            entity.Property(m => m.SentAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()").IsRequired();
            entity.Property(m => m.IsRead).HasDefaultValue(false);
            entity.Property(m => m.IsDeleted).HasDefaultValue(false);


            entity.HasOne(m => m.ChatRoom)
                  .WithMany()
                  .HasForeignKey(m => m.ChatRoomId);
        }
    }

}
