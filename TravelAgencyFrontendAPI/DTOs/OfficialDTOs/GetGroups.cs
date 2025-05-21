namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class GetGroups
    {
        public int GroupId { get; set; }
        public DateTime? Departure { get; set; }
        public DateTime? Return { get; set; }
        public int? TotalSeats { get; set; }
        public int? AvailableSeats { get; set; }
        public string? GroupStatus { get; set; }
    }
}
