using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class ResetPasswordConfig : IEntityTypeConfiguration<ResetPassword>
    {
        public void Configure(EntityTypeBuilder<ResetPassword> entity)
        {
            entity.ToTable("T_ResetPassword");
            entity.HasKey(r => r.TokenId);

            entity.Property(r => r.Token).HasMaxLength(100).IsRequired();
            entity.Property(r => r.CreatedTime).HasColumnType("datetime").HasDefaultValueSql("GETDATE()").IsRequired();
            entity.Property(r => r.ExpireTime).HasColumnType("datetime").IsRequired();
            entity.Property(r => r.IsUsed).HasDefaultValue(false);

            entity.HasOne(r => r.Member)
                  .WithMany()
                  .HasForeignKey(r => r.MemberId);
        }
    }
}
