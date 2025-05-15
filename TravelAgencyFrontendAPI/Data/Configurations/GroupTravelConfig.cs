using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class GroupTravelConfig : IEntityTypeConfiguration<GroupTravel>
    {
        public void Configure(EntityTypeBuilder<GroupTravel> entity)
        {
            entity.ToTable("T_GroupTravel");

            entity.HasKey(e => e.GroupTravelId);

            entity.Property(e => e.DepartureDate).HasColumnType("date").IsRequired(false);
            entity.Property(e => e.ReturnDate).HasColumnType("date").IsRequired(false);
            entity.Property(e => e.TotalSeats).IsRequired(false);
            entity.Property(e => e.SoldSeats).IsRequired(false);
            entity.Property(e => e.OrderDeadline).HasColumnType("date").IsRequired(false);
            entity.Property(e => e.MinimumParticipants).IsRequired(false);
            entity.Property(e => e.GroupStatus).HasMaxLength(10).IsRequired(false);
            entity.Property(e => e.CreatedAt).HasColumnType("date").HasDefaultValueSql("GETDATE()").IsRequired(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").IsRequired(false);
            entity.Property(e => e.RecordStatus).HasMaxLength(10).IsRequired(false);

            entity.HasOne(e => e.OfficialTravelDetail)
                    .WithMany(d => d.GroupTravels)
                    .HasForeignKey(e => e.OfficialTravelDetailId);

        }
    }

}
