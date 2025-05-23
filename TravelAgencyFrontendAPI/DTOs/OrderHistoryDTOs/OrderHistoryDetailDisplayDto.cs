using TravelAgencyFrontendAPI.DTOs.OrderHistoryDTOs;

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    // 主 DTO：用於前端點擊展開訂單時，顯示完整明細
    public class OrderHistoryDetailDisplayDto
    {
        // 行程相關資訊
        public string Description { get; set; } = null!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalPeople => Participants?.Count ?? 0;
        public int AdultCount { get; set; }  // 若你日後分年齡層
        public int ChildCount { get; set; }

        // 訂購人與旅客資訊
        public string OrdererName { get; set; } = null!;
        public string OrdererPhone { get; set; } = null!;
        public string OrdererEmail { get; set; } = null!;
        public string? Note { get; set; }
        public List<OrderHistoryParticipantDto> Participants { get; set; } = new();


        // 付款與發票資訊
        public string? PaymentMethod { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderHistoryInvoiceDto? Invoice { get; set; }
    }
}
