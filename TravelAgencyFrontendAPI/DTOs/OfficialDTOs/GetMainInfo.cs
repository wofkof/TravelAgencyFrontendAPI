namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class GetMainInfo:baseDTO
    {
        public int? Number { get; set; }
        public decimal? Price { get; set; }
        public DateTime? Departure { get; set; }
        public DateTime? Return { get; set; }
        public int? TotalSeats { get; set; }
        public int? AvailableSeats { get; set; }
    }
}
