namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class SearchOutput : baseDTO
    {
        public decimal? Price { get; set; }
        public int DetailId { get; set; }
        public int GroupId { get; set; }

        public DateTime? DepartureDate { get; set; }
        public string? Status { get; set; }
    }
}
