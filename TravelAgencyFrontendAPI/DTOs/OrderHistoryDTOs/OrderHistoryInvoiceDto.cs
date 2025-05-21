namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    // 子 DTO：用於顯示發票資訊
    public class OrderHistoryInvoiceDto
    {
        public string? InvoiceNumber { get; set; }
        public string InvoiceStatus { get; set; } = null!;
        public string InvoiceType { get; set; } = null!;
        public string? BuyerName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? BuyerUniformNumber { get; set; }
        public string? InvoiceFileURL { get; set; }
        public string? Note { get; set; }
    }
}
