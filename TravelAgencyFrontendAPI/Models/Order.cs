namespace TravelAgencyFrontendAPI.Models
{
    public enum OrderCategory
    {
        Official,
        Custom
    }

    public enum OrderStatus
    {
        New,
        Paid,
        Cancelled,
        Refunded
    }

    public enum PaymentMethod
    {
        CreditCard,
        BankTransfer
    }
    public class Order
    {
        public int OrderId { get; set; }
        public int MemberId { get; set; }
        public int ItemId { get; set; } 
        public int ParticipantId { get; set; }

        public OrderCategory Category { get; set; }

        public DateTime CreatedAt { get; set; }
        public int ParticipantsCount { get; set; }
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public DateTime? PaymentDate { get; set; }
        public string? Note { get; set; }

        public Member Member { get; set; }
        public Participant Participant { get; set; }
    }

}
