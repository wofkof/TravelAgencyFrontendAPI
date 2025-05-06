namespace TravelAgencyFrontendAPI.Models
{
    public class GroupTravel
    {
        public int GroupTravelId { get; set; }
        public int OfficialTravelDetailId { get; set; }

        public DateTime? DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? TotalSeats { get; set; }
        public int? SoldSeats { get; set; }
        public DateTime? OrderDeadline { get; set; }
        public int? MinimumParticipants { get; set; }
        public string? GroupStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? RecordStatus { get; set; }

        public OfficialTravelDetail OfficialTravelDetail { get; set; }
        public ICollection<TravelRecord> TravelRecords { get; set; } = new List<TravelRecord>();
    }
}


