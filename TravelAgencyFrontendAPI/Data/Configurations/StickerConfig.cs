using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class StickerConfig : IEntityTypeConfiguration<Sticker>
    {
        public void Configure(EntityTypeBuilder<Sticker> entity)
        {
            entity.ToTable("T_Sticker");

            entity.HasKey(s => s.StickerId);

            entity.Property(s => s.Category).HasMaxLength(50).IsRequired();
            entity.Property(s => s.ImagePath).HasMaxLength(255).IsRequired();
        }
    }

}
