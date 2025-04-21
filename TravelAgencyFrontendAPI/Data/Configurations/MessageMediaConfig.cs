using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class MessageMediaConfig : IEntityTypeConfiguration<MessageMedia>
    {
        public void Configure(EntityTypeBuilder<MessageMedia> entity)
        {
            entity.ToTable("T_MessageMedia");

            entity.HasKey(m => m.MediaId);

            entity.Property(m => m.MediaType).HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(m => m.FilePath).HasMaxLength(255).IsRequired();
            entity.Property(m => m.DurationInSeconds).IsRequired(false);

            entity.HasOne(m => m.Message)
                  .WithMany()
                  .HasForeignKey(m => m.MessageId);
        }
    }

}
