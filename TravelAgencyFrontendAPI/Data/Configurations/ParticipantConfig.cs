using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class ParticipantConfig : IEntityTypeConfiguration<Participant>
    {
        public void Configure(EntityTypeBuilder<Participant> entity)
        {
            entity.ToTable("T_Participant");
            entity.HasKey(p => p.ParticipantId);

            entity.Property(p => p.Name).HasMaxLength(50).IsRequired();

            entity.Property(p => p.Phone).HasMaxLength(20).IsRequired();
            entity.HasIndex(p => p.Phone).IsUnique();

            entity.Property(p => p.IdNumber).HasMaxLength(20).IsRequired();
            entity.HasIndex(p => p.IdNumber).IsUnique();

            entity.Property(p => p.BirthDate).HasColumnType("date").IsRequired(false);

            entity.Property(p => p.EnglishName).HasMaxLength(100).IsRequired();

            entity.Property(p => p.PassportNumber).HasMaxLength(20).IsRequired();
            entity.HasIndex(p => p.PassportNumber).IsUnique();

            entity.Property(p => p.IssuedPlace).HasMaxLength(50).IsRequired();
            
            entity.Property(p => p.PassportIssueDate).HasColumnType("date").IsRequired();

            entity.HasOne(p => p.Member)
                  .WithMany(m => m.Participants)
                  .HasForeignKey(p => p.MemberId);
        }
    }
}
