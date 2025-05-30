using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.OrderHistoryDTOs
{
    public class OrderHistoryListItemDto
    {
        public int OrderId { get; set; }
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public string? PrimaryProductName { get; set; } // 例如：行程主標題

        public OrderStatus OriginalStatus { get; set; }
        public string Status { get; set; } = string.Empty;

        public DateTime? ExpiresAt { get; set; } // 猶豫期/付款到期時間
        public decimal TotalAmount { get; set; } // 訂單總金額
        public string MerchantTradeNo { get; set; } // 廠商交易編號
    }
}
