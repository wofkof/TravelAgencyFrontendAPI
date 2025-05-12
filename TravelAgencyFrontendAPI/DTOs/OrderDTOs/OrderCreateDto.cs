using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using TravelAgencyFrontendAPI.Models; // For PaymentMethod enum

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class OrderCreateDto
    {
        [Required(ErrorMessage = "訂單總金額為必填")]
        [Range(0.01, double.MaxValue, ErrorMessage = "總金額必須大於0")]
        public decimal TotalAmount { get; set; }

        [StringLength(500, ErrorMessage = "訂單備註過長")]
        public string? OrderNotes { get; set; }

        [Required(ErrorMessage = "訂購人資訊為必填")]
        public OrdererInfoDto OrdererInfo { get; set; }

        [Required(ErrorMessage = "至少需要一位旅客")]
        [MinLength(1, ErrorMessage = "至少需要一位旅客")]
        public List<OrderParticipantDto> Participants { get; set; } = new List<OrderParticipantDto>();

        [Required(ErrorMessage = "發票請求資訊為必填")]
        public OrderInvoiceRequestDto InvoiceRequestInfo { get; set; }

        [Required(ErrorMessage = "必須選擇付款方式")]
        public PaymentMethod SelectedPaymentMethod { get; set; } // 使用者在前端選擇的付款方式
    }
}