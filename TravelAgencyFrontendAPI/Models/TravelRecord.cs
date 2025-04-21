namespace TravelAgencyFrontendAPI.Models
{
    public class TravelRecord
    {
        public int TravelRecordId { get; set; }
        public int OrderId { get; set; }

        public decimal TotalAmount { get; set; }
        public int TotalParticipants { get; set; }
        public DateTime CreatedAt { get; set; }

        public Order Order { get; set; }
    }

}
