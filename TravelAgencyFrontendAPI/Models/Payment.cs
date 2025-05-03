namespace TravelAgencyFrontendAPI.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int DocumentOrderId { get; set; }

        public DateTime PaymentDeadline { get; set; }

        public PaymentMethod PaymentMethod { get; set; } 

        public DocumentOrderDetails DocumentOrderDetails { get; set; } = null!;
    }
}
