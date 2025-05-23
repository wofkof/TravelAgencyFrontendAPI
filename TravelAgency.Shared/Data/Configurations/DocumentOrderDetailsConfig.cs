using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class DocumentOrderDetailsConfig : IEntityTypeConfiguration<DocumentOrderDetails>
    {
        public void Configure(EntityTypeBuilder<DocumentOrderDetails> entity)
        {
            entity.ToTable("T_DocumentOrderDetails");

            entity.HasKey(e => e.DocumentOrderId);

            entity.Property(e => e.MemberId).IsRequired();

            //entity.Property(e => e.ApplicationType)
            //    .IsRequired()
            //    .HasMaxLength(10);
            entity.Property(e => e.RequiredData).HasColumnType("text");
            entity.Property(e => e.SubmissionMethod).HasMaxLength(500);
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.DepartureDate).HasColumnType("date");
            entity.Property(e => e.ProcessingCount).IsRequired();
            entity.Property(e => e.ChineseLastName).HasMaxLength(50);
            entity.Property(e => e.ChineseFirstName).HasMaxLength(50);
            entity.Property(e => e.EnglishLastName).HasMaxLength(50);
            entity.Property(e => e.EnglishFirstName).HasMaxLength(50);
            entity.Property(e => e.BirthDate).HasColumnType("date");

            //entity.Property(e => e.ApplicationType)
            //    .HasConversion<string>()
            //    .HasMaxLength(10)
            //    .IsRequired();

            entity.HasOne(e => e.DocumentApplicationForm)
                .WithMany()
                .HasForeignKey(e => e.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PickupMethod)
                .WithMany()
                .HasForeignKey(e => e.PickupMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PickupInformation)
                .WithMany()
                .HasForeignKey(e => e.PickupInfoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Agency)
                .WithMany()
                .HasForeignKey(e => e.AgencyCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Member)
                .WithMany()
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
