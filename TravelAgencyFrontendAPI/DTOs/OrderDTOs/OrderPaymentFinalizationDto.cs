using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models; 

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{

    public class OrderPaymentFinalizationDto
    {
        [Required(ErrorMessage = "必須選擇付款方式")]
        public PaymentMethod SelectedPaymentMethod { get; set; }

        [Required(ErrorMessage = "發票請求資訊為必填")]
        public OrderInvoiceRequestDto InvoiceRequestInfo { get; set; } = null!; // << 使用 OrderInvoiceRequestDto >>
        public int MemberId { get; set; }
    }

}
