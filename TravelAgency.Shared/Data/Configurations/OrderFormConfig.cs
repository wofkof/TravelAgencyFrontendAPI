using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data.Configurations
{
    public class OrderFormConfig : IEntityTypeConfiguration<OrderForm>
    {
        public void Configure(EntityTypeBuilder<OrderForm> entity)
        {
            entity.ToTable("T_OrderForm");

            entity.HasKey(e => e.OrderId);

            entity.Property(e => e.OrderId).HasColumnName("order_id");

            entity.Property(e => e.MemberId).HasColumnName("member_id");
            entity.HasOne(e => e.Member)
                  .WithMany()
                  .HasForeignKey(e => e.MemberId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.DocumentMenuId).HasColumnName("document_menu_id");
            entity.HasOne(e => e.DocumentMenu)
                  .WithMany()
                  .HasForeignKey(e => e.DocumentMenuId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.DepartureDate).HasColumnName("departure_date");

            entity.Property(e => e.ProcessingQuantity)
                  .HasColumnName("processing_quantity");

            entity.Property(e => e.ChineseSurname)
                  .HasColumnName("chinese_surname")
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.ChineseName)
                  .HasColumnName("chinese_name")
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.EnglishSurname)
                  .HasColumnName("english_surname")
                  .HasMaxLength(20);

            entity.Property(e => e.EnglishName)
                  .HasColumnName("english_name")
                  .HasMaxLength(20);

            entity.Property(e => e.Gender)
                  .HasColumnName("gender")
                  .HasConversion<string>()
                  .HasMaxLength(10);

            entity.Property(e => e.BirthDate)
                  .HasColumnName("birth_date")
                  .IsRequired();

            entity.Property(e => e.ContactPersonName)
                  .HasColumnName("contact_person_name")
                  .HasMaxLength(10);

            entity.Property(e => e.ContactPersonEmail)
                  .HasColumnName("contact_person_email")
                  .HasMaxLength(30);

            entity.Property(e => e.ContactPersonPhoneNumber)
                  .HasColumnName("contact_person_phonenumber")
                  .HasMaxLength(20);

            entity.Property(e => e.PickupMethodOption)
                  .HasColumnName("Pickup_method_option")
                  .HasConversion<string>()
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(e => e.MailingCity)
                  .HasColumnName("mailing_city")
                  .HasMaxLength(50);

            entity.Property(e => e.MailingDetailAddress)
                  .HasColumnName("mailing_detail_address")
                  .HasMaxLength(50);

            entity.Property(e => e.StoreDetailAddress)
                  .HasColumnName("store_detail_address")
                  .HasMaxLength(50);

            entity.Property(e => e.TaxIdOption)
                  .HasColumnName("Tax_ID_option")
                  .HasConversion<string>()
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(e => e.CompanyName)
                  .HasColumnName("company_name")
                  .HasMaxLength(20);

            entity.Property(e => e.TaxIdNumber)
                  .HasColumnName("tax_id_number")
                  .HasMaxLength(20);

            entity.Property(e => e.OrderCreationTime)
                  .HasColumnName("order_creation_time")
                  .IsRequired();
        }
    }

}
