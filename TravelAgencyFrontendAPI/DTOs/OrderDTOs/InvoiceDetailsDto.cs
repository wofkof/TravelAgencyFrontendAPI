namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class InvoiceDetailsDto
    {
        public int? RtnCode { get; set; }
        public string RtnMsg { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string RandomNumber { get; set; }

        public string InvoiceType { get; set; } // "Triplet", "ElectronicInvoice"
        public string BuyerName { get; set; }
        public string BuyerUniformNumber { get; set; }
        public decimal? TotalAmount { get; set; }
        public string InvoiceItemDesc { get; set; }
        public string Note { get; set; }

        // public string InvoiceStatus { get; set; } // 如果需要顯示發票狀態文字
    }
}
