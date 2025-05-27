using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class DocumentMenuConfig : IEntityTypeConfiguration<DocumentMenu>
    {
        public void Configure(EntityTypeBuilder<DocumentMenu> entity)
        {
            entity.ToTable("T_DocumentMenu");

            entity.HasKey(e => e.MenuId);

            entity.Property(e => e.MenuId)
                  .HasColumnName("menu_id");

            entity.Property(e => e.RocPassportOption)
                  .HasColumnName("roc_passport_option")
                  .HasMaxLength(50);

            entity.Property(e => e.ForeignVisaOption)
                  .HasColumnName("foreign_visa_option")
                  .HasMaxLength(50);

            entity.Property(e => e.ApplicationType)
                  .HasColumnName("application_type")
                  .HasConversion<string>()
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(e => e.CaseType)
                  .HasColumnName("case_type")
                  .HasConversion<string>()
                  .HasMaxLength(10);

            entity.Property(e => e.ProcessingItem)
                  .HasColumnName("processing_item")
                  .HasMaxLength(100);

            entity.Property(e => e.ProcessingDays)
                  .HasColumnName("processing_days")
                  .HasMaxLength(10);

            entity.Property(e => e.DocumentValidityPeriod)
                  .HasColumnName("document_validity_period")
                  .HasMaxLength(10);

            entity.Property(e => e.StayDuration)
                  .HasColumnName("stay_duration")
                  .HasMaxLength(10);

            entity.Property(e => e.Fee)
                  .HasColumnName("fee")
                  .HasMaxLength(10);
        }
    }

}
