﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class CollectConfig : IEntityTypeConfiguration<Collect>
    {
        public void Configure(EntityTypeBuilder<Collect> entity)
        {
            entity.ToTable("T_Collect");

            entity.HasKey(e => e.CollectId);

            entity.Property(e => e.TravelType)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.TravelId)
                  .IsRequired();

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime")
                  .HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Member)
                  .WithMany(m => m.Collects)
                  .HasForeignKey(e => e.MemberId);

            entity.HasIndex(e => new { e.MemberId, e.TravelType, e.TravelId })
                  .IsUnique();
        }
    }

}
