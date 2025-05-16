using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class OrderParticipantConfig : IEntityTypeConfiguration<OrderParticipant>
    {
        public void Configure(EntityTypeBuilder<OrderParticipant> entity)
        {
            entity.ToTable("T_OrderParticipant");

            entity.HasKey(e => e.OrderParticipantId);

            entity.Property(e => e.OrderParticipantId)
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.BirthDate)
                  .HasColumnType("date")
                  .IsRequired();

            entity.Property(e => e.IdNumber)
                  .IsRequired()
                  .HasMaxLength(20);
            entity.HasIndex(e => e.IdNumber).IsUnique();

            entity.Property(e => e.Gender)
                  .IsRequired()
                  .HasMaxLength(10);

            entity.Property(e => e.Phone)
                  .IsRequired()
                  .HasMaxLength(20);
            entity.HasIndex(e => e.Phone).IsUnique();

            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.DocumentType)
                  .HasMaxLength(20)
                  .HasConversion<string>()
                  .IsRequired();

            entity.Property(e => e.DocumentNumber)
                  .HasMaxLength(50)
                  .IsRequired(false);

            entity.Property(e => e.PassportSurname)
                  .HasMaxLength(50)
                  .IsRequired(false);

            entity.Property(e => e.PassportGivenName)
                  .HasMaxLength(50)
                  .IsRequired(false);

            entity.Property(e => e.PassportExpireDate)
                  .HasColumnType("date")
                  .IsRequired(false);

            entity.Property(e => e.Nationality)
                  .HasMaxLength(50)
                  .IsRequired(false);

            entity.Property(e => e.Note)
                  .HasMaxLength(255)
                  .IsRequired(false);

            entity.Property(e => e.Gender)
                  .HasConversion<string>()
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(e => e.DocumentType)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderParticipants)
                  .HasForeignKey(e => e.OrderId);

        }
    }
}
