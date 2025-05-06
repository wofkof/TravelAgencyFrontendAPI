using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class MemberConfig : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.ToTable("T_Member");

            builder.HasKey(m => m.MemberId);

            builder.Property(m => m.MemberId)
                   .UseIdentityColumn(11110, 1);

            builder.Property(m => m.Name)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(m => m.Birthday)
                   .HasColumnType("date")
                   .IsRequired(false);

            builder.Property(m => m.Email)
                   .HasMaxLength(100)
                   .IsRequired();
            builder.HasIndex(m => m.Email).IsUnique();

            builder.Property(m => m.Phone)
                   .HasMaxLength(20)
                   .IsRequired();
            builder.HasIndex(m => m.Phone).IsUnique();

            builder.Property(m => m.Gender)
                   .HasConversion<string>()
                   .HasMaxLength(10)
                   .HasDefaultValueSql("N'Other'")
                   .IsRequired();

            builder.Property(m => m.IdNumber)
                   .HasMaxLength(20)
                   .IsRequired(false);

            builder.Property(m => m.PassportSurname)
                   .HasMaxLength(50)
                   .IsRequired(false);

            builder.Property(m => m.PassportGivenName)
                   .HasMaxLength(50)
                   .IsRequired(false);

            builder.Property(m => m.PassportExpireDate)
                   .HasColumnType("date")
                   .IsRequired(false);

            builder.Property(m => m.Nationality)
                   .HasMaxLength(50)
                   .IsRequired(false);

            builder.Property(m => m.DocumentType)
                   .HasConversion<string>()
                   .HasDefaultValueSql("N'Passport'")
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(m => m.DocumentNumber)
                   .HasMaxLength(50)
                   .IsRequired(false);

            builder.Property(m => m.Address)
                   .HasMaxLength(255)
                   .IsRequired(false);

            builder.Property(m => m.PasswordHash)
                   .HasMaxLength(255)
                   .IsRequired();

            builder.Property(m => m.PasswordSalt)
                   .HasMaxLength(255)
                   .IsRequired();

            builder.Property(m => m.GoogleId)
                   .HasMaxLength(100)
                   .IsRequired(false);

            builder.Property(m => m.RegisterDate)
                   .HasDefaultValueSql("GETDATE()")
                   .IsRequired();

            builder.Property(m => m.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(MemberStatus.Active)
                   .IsRequired();

            builder.Property(m => m.RememberToken)
                   .HasMaxLength(255)
                   .IsRequired(false);

            builder.Property(m => m.RememberExpireTime)
                   .HasColumnType("datetime")
                   .IsRequired(false); 

            builder.Property(m => m.IsBlacklisted)
                   .HasDefaultValue(false);

            builder.Property(m => m.Note)
                   .HasMaxLength(255)
                   .IsRequired(false);

            builder.Property(m => m.UpdatedAt)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("GETDATE()")
                   .IsRequired(false);

            builder.Property(m => m.DeletedAt)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("GETDATE()")
                   .IsRequired(false);
        }
    }
}
