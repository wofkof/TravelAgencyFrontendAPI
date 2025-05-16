using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models; // For InvoiceOption enum

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class OrderInvoiceRequestDto
    {
        [Required(ErrorMessage = "祇布匡兜ゲ恶")]
        public InvoiceOption InvoiceOption { get; set; } // Personal (羛), Company (羛)

        // 癸莱 Order Model い祇布逆
        [EmailAddress(ErrorMessage = "叫块Τ祇布盚癳筿獺絚")]
        public string? InvoiceDeliveryEmail { get; set; } // 祇布盚癳Email (盽ノ筿祇布)

        // そ祇布 (InvoiceOption.Company)
        [StringLength(10)]
        public string? InvoiceUniformNumber { get; set; } // そ参絪

        [StringLength(100)]
        public string? InvoiceTitle { get; set; } // 祇布╋繷

        public bool InvoiceAddBillingAddr { get; set; } = false;

        [StringLength(200)]
        public string? InvoiceBillingAddress { get; set; }
    }
}