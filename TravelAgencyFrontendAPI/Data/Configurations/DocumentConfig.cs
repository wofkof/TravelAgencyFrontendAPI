using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class DocumentConfig : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> entity)
        {
            entity.ToTable("T_Documents");

            entity.HasKey(e => e.DocumentId);
            entity.Property(e => e.DocumentName).HasMaxLength(50).IsRequired();
        }
    }

}
