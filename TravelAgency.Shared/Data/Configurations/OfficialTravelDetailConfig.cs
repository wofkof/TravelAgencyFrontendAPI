﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class OfficialTravelDetailConfig : IEntityTypeConfiguration<OfficialTravelDetail>
    {
        public void Configure(EntityTypeBuilder<OfficialTravelDetail> entity)
        {
            entity.ToTable("T_OfficialTravelDetail");

            entity.HasKey(d => d.OfficialTravelDetailId);

            entity.Property(d => d.TravelNumber).IsRequired(false);
            entity.Property(d => d.AdultPrice).HasColumnType("money").IsRequired(false);
            entity.Property(d => d.ChildPrice).HasColumnType("money").IsRequired(false);
            entity.Property(d => d.BabyPrice).HasColumnType("money").IsRequired(false);
            entity.Property(d => d.UpdatedAt).HasColumnType("datetime").IsRequired(false);

            entity.Property(d => d.State)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired(false);

            entity.HasOne(d => d.OfficialTravel)
                  .WithMany(o => o.OfficialTravelDetails)
                  .HasForeignKey(d => d.OfficialTravelId);
        }
    }

}
