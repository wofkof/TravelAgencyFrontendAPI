using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelAgency.Shared.Models
{
    public enum GenderEnum
    {
        Male,
        Female
    }

    public enum PickupMethodEnum
    {
        郵寄,
        門市
    }

    public enum TaxIdOptionEnum
    {
        需要,
        不需要
    }
    public class OrderForm
    {
        public int OrderId { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public int DocumentMenuId { get; set; }
        public DocumentMenu DocumentMenu { get; set; } = null!;

        public DateTime DepartureDate { get; set; }

        public ushort ProcessingQuantity { get; set; }

        public string ChineseSurname { get; set; } = null!;
        public string ChineseName { get; set; } = null!;

        public string? EnglishSurname { get; set; }
        public string? EnglishName { get; set; }

        public GenderEnum? Gender { get; set; }

        public DateTime BirthDate { get; set; }

        public string? ContactPersonName { get; set; }
        public string? ContactPersonEmail { get; set; }
        public string? ContactPersonPhoneNumber { get; set; }

        public PickupMethodEnum PickupMethodOption { get; set; }

        public string? MailingCity { get; set; }
        public string? MailingDetailAddress { get; set; }
        public string? StoreDetailAddress { get; set; }

        public TaxIdOptionEnum TaxIdOption { get; set; }

        public string? CompanyName { get; set; }
        public string? TaxIdNumber { get; set; }

        public DateTime OrderCreationTime { get; set; }
    }
}
