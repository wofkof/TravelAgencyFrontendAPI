using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class ChatRoomConfig : IEntityTypeConfiguration<ChatRoom>
    {
        public void Configure(EntityTypeBuilder<ChatRoom> entity)
        {
            entity.ToTable("T_ChatRoom");

            entity.HasKey(c => c.ChatRoomId);

            entity.Property(c => c.IsBlocked).HasDefaultValue(false);
            entity.Property(c => c.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()").IsRequired();
            entity.Property(c => c.LastMessageAt).HasColumnType("datetime").IsRequired(false);

            entity.HasOne(c => c.Employee)
                  .WithMany(e => e.ChatRooms)
                  .HasForeignKey(c => c.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Member)
                  .WithMany(m => m.ChatRooms)
                  .HasForeignKey(c => c.MemberId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => new { c.EmployeeId, c.MemberId })
                  .IsUnique();
        }
    }

}
