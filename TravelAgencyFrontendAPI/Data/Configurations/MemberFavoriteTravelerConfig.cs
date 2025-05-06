using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class MemberFavoriteTravelerConfig : IEntityTypeConfiguration<MemberFavoriteTraveler>
    {
        public void Configure(EntityTypeBuilder<MemberFavoriteTraveler> entity)
        {
            entity.ToTable("T_MemberFavoriteTraveler");
            entity.HasKey(p => p.FavoriteTravelerId);
            entity.Property(m => m.FavoriteTravelerId)
                  .UseIdentityColumn(20000, 1);

            entity.Property(p => p.Name).HasMaxLength(50).IsRequired();

            entity.Property(p => p.Gender).HasMaxLength(10).HasConversion<string>().HasDefaultValueSql("N'Other'").IsRequired();

            entity.Property(p => p.Phone).HasMaxLength(20).IsRequired();
            entity.HasIndex(p => p.Phone).IsUnique();

            entity.Property(p => p.Email).HasMaxLength(100).IsRequired();
            entity.HasIndex(p => p.Email).IsUnique();

            entity.Property(p => p.DocumentType).HasMaxLength(20).HasConversion<string>().HasDefaultValueSql("N'Passport'").IsRequired();
            entity.Property(p => p.DocumentNumber).HasMaxLength(50).IsRequired(false);

            entity.Property(p => p.PassportSurname).HasMaxLength(50).IsRequired(false);
            entity.Property(p => p.PassportGivenName).HasMaxLength(50).IsRequired(false);
            entity.Property(p => p.PassportExpireDate).HasColumnType("date").IsRequired(false);

            entity.Property(p => p.Nationality).HasMaxLength(50).IsRequired(false);

            entity.Property(p => p.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()").IsRequired();
            entity.Property(p => p.UpdatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()").IsRequired(false);

            entity.Property(p => p.Note).HasMaxLength(255).IsRequired(false);
            entity.Property(p => p.Status).HasMaxLength(20).HasDefaultValue(FavoriteStatus.Active).HasConversion<string>().IsRequired();

            entity.Property(p => p.IdNumber).HasMaxLength(20).IsRequired();
            entity.HasIndex(p => p.IdNumber).IsUnique();

            entity.Property(p => p.BirthDate).HasColumnType("date").IsRequired();

            entity.HasOne(p => p.Member)
                  .WithMany(m => m.MemberFavoriteTravelers)
                  .HasForeignKey(p => p.MemberId);
        }
    }
}
