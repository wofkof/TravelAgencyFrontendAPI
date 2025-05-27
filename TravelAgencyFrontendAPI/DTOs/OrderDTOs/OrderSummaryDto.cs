namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; } = null!;
        public decimal TotalAmount { get; set; }

        //public string SelectedPaymentMethod { get; set; }
        public string MerchantTradeNo { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }    // 訂單失效時間
    }
}
