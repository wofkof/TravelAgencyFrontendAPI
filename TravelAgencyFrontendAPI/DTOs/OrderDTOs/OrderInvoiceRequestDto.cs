using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models; // For InvoiceOption enum

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class OrderInvoiceRequestDto
    {
        [Required(ErrorMessage = "必須選擇發票選項")]
        public InvoiceOption InvoiceOption { get; set; }

        // Email: 個人發票時為 CloudInvoiceDeliveryEmail，公司發票時為 CompanyInvoiceEmail
        [EmailAddress(ErrorMessage = "Email格式不正確")]
        [StringLength(100, ErrorMessage = "Email過長")]
        public string? InvoiceDeliveryEmail { get; set; }

        [StringLength(8, MinimumLength = 8, ErrorMessage = "統一編號必須為8碼")]
        public string? InvoiceUniformNumber { get; set; } // 公司統編

        [StringLength(100, ErrorMessage = "發票抬頭過長")]
        public string? InvoiceTitle { get; set; } // 公司發票抬頭

        public bool InvoiceAddBillingAddr { get; set; } = false;

        [StringLength(200, ErrorMessage = "帳單地址過長")]
        public string? InvoiceBillingAddress { get; set; }
    }
}