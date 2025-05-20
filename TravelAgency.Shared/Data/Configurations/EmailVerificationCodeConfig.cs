using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class EmailVerificationCodeConfig : IEntityTypeConfiguration<EmailVerificationCode>
    {
        public void Configure(EntityTypeBuilder<EmailVerificationCode> entity)
        {
            entity.ToTable("T_EmailVerificationCode");
            entity.HasKey(r => r.VerificationId);

            entity.Property(r => r.Email).HasMaxLength(100).IsRequired();
            entity.Property(r => r.VerificationCode)
                .HasMaxLength(10).IsRequired();
            entity.Property(r => r.VerificationType)
                .HasConversion<string>() 
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(r => r.IsVerified)
                .HasDefaultValue(false)
                 .IsRequired(); 

            entity.Property(e => e.CreatedAt)
                 .HasColumnType("datetime")
                 .HasDefaultValueSql("GETDATE()")
                 .IsRequired();

            entity.Property(e => e.ExpireAt)
                  .HasColumnType("datetime")
                  .IsRequired();
        }
    }
}
