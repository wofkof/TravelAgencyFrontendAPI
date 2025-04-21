using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class VisaTypeConfig : IEntityTypeConfiguration<VisaType>
    {
        public void Configure(EntityTypeBuilder<VisaType> entity)
        {
            entity.ToTable("T_VisaTypes");

            entity.HasKey(e => e.VisaTypeId);
            entity.Property(e => e.VisaTypeName).HasMaxLength(50).IsRequired();
        }
    }

}
