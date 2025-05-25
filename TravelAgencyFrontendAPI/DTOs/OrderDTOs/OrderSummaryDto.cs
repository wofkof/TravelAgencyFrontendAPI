namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public string SelectedPaymentMethod { get; set; }
    }
}
