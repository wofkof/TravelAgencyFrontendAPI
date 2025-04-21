using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data.Configurations
{
    public class VisaInformationConfig : IEntityTypeConfiguration<VisaInformation>
    {
        public void Configure(EntityTypeBuilder<VisaInformation> entity)
        {
            entity.ToTable("T_VisaInformation");

            entity.HasKey(e => e.VisaInfoId);

            entity.HasOne(e => e.Country)
                  .WithMany()
                  .HasForeignKey(e => e.CountryId);

            entity.HasOne(e => e.VisaType)
                  .WithMany()
                  .HasForeignKey(e => e.VisaTypeId);
        }
    }

}
