using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
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
                  .WithMany()
                  .HasForeignKey(c => c.EmployeeId);

            entity.HasOne(c => c.Member)
                  .WithMany()
                  .HasForeignKey(c => c.MemberId);
        }
    }

}
