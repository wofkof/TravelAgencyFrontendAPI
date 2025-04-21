namespace TravelAgencyFrontendAPI.Models
{
    public enum DetailState
    {
        Locked,
        Deleted
    }
    public class OfficialTravelDetail
    {
        public int OfficialTravelDetailId { get; set; }
        public int OfficialTravelId { get; set; }

        public int? TravelNumber { get; set; }
        public decimal? AdultPrice { get; set; }
        public decimal? ChildPrice { get; set; }
        public decimal? BabyPrice { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public DetailState? State { get; set; }

        public OfficialTravel OfficialTravel { get; set; }
    }

}
