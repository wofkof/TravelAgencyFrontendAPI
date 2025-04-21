using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class RequirementConfig : IEntityTypeConfiguration<Requirement>
    {
        public void Configure(EntityTypeBuilder<Requirement> entity)
        {
            entity.ToTable("T_Requirements");

            entity.HasKey(e => e.RequirementId);
            entity.Property(e => e.RequirementName).HasMaxLength(50).IsRequired();
        }
    }

}
