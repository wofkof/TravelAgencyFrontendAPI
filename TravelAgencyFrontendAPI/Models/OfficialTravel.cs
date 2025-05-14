namespace TravelAgencyFrontendAPI.Models
{
    public enum TravelCategory
    {
        Domestic,
        Foreign,
        CruiseShip
    }

    public enum TravelStatus
    {
        Active,
        Hidden,
        Deleted
    }
    public class OfficialTravel
    {
        public int OfficialTravelId { get; set; }
        public int CreatedByEmployeeId { get; set; }
        public int RegionId { get; set; }

        public int ItemId { get; set; } 
        public TravelCategory Category { get; set; }

        public string Title { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }
        public string? Description { get; set; }
        public int? TotalTravelCount { get; set; }
        public int? TotalDepartureCount { get; set; }
        public int? Days { get; set; }
        public string? CoverPath { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public TravelStatus? Status { get; set; }

        public Employee CreatedByEmployee { get; set; }
        public Region Region { get; set; }

        public ICollection<OfficialTravelDetail> OfficialTravelDetails { get; set; } = new List<OfficialTravelDetail>();
    }

}
