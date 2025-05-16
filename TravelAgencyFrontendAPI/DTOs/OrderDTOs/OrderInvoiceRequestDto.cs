using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models; // For InvoiceOption enum

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class OrderInvoiceRequestDto
    {
        [Required(ErrorMessage = "�o���ﶵ������")]
        public InvoiceOption InvoiceOption { get; set; } // Personal (�G�p), Company (�T�p)

        // �H�U���� Order Model �����o�����
        [EmailAddress(ErrorMessage = "�п�J���Ī��o���H�e�q�l�H�c")]
        public string? InvoiceDeliveryEmail { get; set; } // �o���H�eEmail (�`�Ω�q�l�o��)

        // ���q�o�� (InvoiceOption.Company)
        [StringLength(10)]
        public string? InvoiceUniformNumber { get; set; } // ���q�νs

        [StringLength(100)]
        public string? InvoiceTitle { get; set; } // �o�����Y

        public bool InvoiceAddBillingAddr { get; set; } = false;

        [StringLength(200)]
        public string? InvoiceBillingAddress { get; set; }
    }
}