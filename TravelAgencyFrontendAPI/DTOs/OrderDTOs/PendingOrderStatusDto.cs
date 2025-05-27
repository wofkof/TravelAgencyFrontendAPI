namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class PendingOrderStatusDto
    {
        public int OrderId { get; set; }
        public string MerchantTradeNo { get; set; }
        public string Status { get; set; } // "Awaiting", "Expired"
        public DateTime? ExpiresAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } // 字串型別，例如 "ECPay_CreditCard"
    }
}
