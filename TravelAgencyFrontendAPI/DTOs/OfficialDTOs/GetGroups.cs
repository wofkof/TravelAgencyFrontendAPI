namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class GetGroups
    {
        public int GroupId { get; set; }
        public int DetailId { get; set; }
        public DateTime? Departure { get; set; }
        public DateTime? Return { get; set; }
        public int? TotalSeats { get; set; }
        public int? AvailableSeats { get; set; }
        public string? GroupStatus { get; set; }
        public decimal? Price { get; set; }
        public int? Number { get; set; }
    }
}
