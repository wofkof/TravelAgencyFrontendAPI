using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class DocumentApplicationFormConfig : IEntityTypeConfiguration<DocumentApplicationForm>
    {
        public void Configure(EntityTypeBuilder<DocumentApplicationForm> entity)
        {
            entity.ToTable("T_DocumentApplicationForm");
            entity.HasKey(e => e.ApplicationId);

            entity.Property(e => e.ApplicationId).IsRequired();
            entity.Property(e => e.MemberId).IsRequired();
            entity.Property(e => e.RegionId).HasMaxLength(10);
            entity.Property(e => e.ApplicationType)
                .IsRequired()
                .HasMaxLength(10);
            entity.Property(e => e.ProcessingItem)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.CaseType)
                .IsRequired()
                .HasMaxLength(10);
            entity.Property(e => e.ProcessingDays).IsRequired();
            entity.Property(e => e.ExpiryDate).HasColumnType("date");
            entity.Property(e => e.StayDuration).HasMaxLength(50);
            entity.Property(e => e.Fee).HasColumnType("decimal(6,2)");

            entity.Property(e => e.ApplicationType).HasConversion<string>().HasMaxLength(10).IsRequired();
            entity.Property(e => e.CaseType).HasConversion<string>().HasMaxLength(10).IsRequired();

            entity.HasOne(e => e.Member)
                .WithMany()
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Region)
                .WithMany()
                .HasForeignKey(e => e.RegionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
