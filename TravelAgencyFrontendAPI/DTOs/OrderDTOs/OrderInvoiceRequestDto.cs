using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models; // For InvoiceOption enum

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class OrderInvoiceRequestDto
    {
        [Required(ErrorMessage = "������ܵo���ﶵ")]
        public InvoiceOption InvoiceOption { get; set; }

        // Email: �ӤH�o���ɬ� CloudInvoiceDeliveryEmail�A���q�o���ɬ� CompanyInvoiceEmail
        [EmailAddress(ErrorMessage = "Email�榡�����T")]
        [StringLength(100, ErrorMessage = "Email�L��")]
        public string? InvoiceDeliveryEmail { get; set; }

        [StringLength(8, MinimumLength = 8, ErrorMessage = "�Τ@�s��������8�X")]
        public string? InvoiceUniformNumber { get; set; } // ���q�νs

        [StringLength(100, ErrorMessage = "�o�����Y�L��")]
        public string? InvoiceTitle { get; set; } // ���q�o�����Y

        public bool InvoiceAddBillingAddr { get; set; } = false;

        [StringLength(200, ErrorMessage = "�b��a�}�L��")]
        public string? InvoiceBillingAddress { get; set; }
    }
}