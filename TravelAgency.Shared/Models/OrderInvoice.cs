namespace TravelAgency.Shared.Models
{
    public enum InvoiceStatus
    {
        Pending, // 未開立發票
        Opened, // 已開立發票
        Voided, // 已作廢發票
    }
    public enum InvoiceType
    {
        ElectronicInvoice,
        Double,
        Triplet
    }
    public class OrderInvoice
    {
        public int InvoiceId { get; set; }
        public int OrderId { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? BuyerName { get; set; }
        public string? InvoiceItemDesc { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public InvoiceType InvoiceType { get; set; } = InvoiceType.ElectronicInvoice;
        public InvoiceStatus InvoiceStatus { get; set; } = InvoiceStatus.Pending;
        public string? BuyerUniformNumber { get; set; }
        public string? InvoiceFileURL { get; set; }
        public string? Note { get; set; }

        public string? RandomCode { get; set; } //綠界4位隨機碼
        public Order Order { get; set; } = null!;
    }
}
