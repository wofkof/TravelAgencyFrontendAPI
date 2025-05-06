namespace TravelAgencyFrontendAPI.Models
{
    public class TravelRecord
    {
        public int TravelRecordId { get; set; }
        public int GroupTravelId { get; set; }
        public int TotalParticipants { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0.00M;
        public DateTime CompletionDate { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public GroupTravel GroupTravel { get; set; } = null!;
    }
}
