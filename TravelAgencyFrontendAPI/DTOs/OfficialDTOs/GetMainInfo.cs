namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class GetMainInfo:baseDTO
    {
        public decimal? Price { get; set; }
        public DateTime? Departure { get; set; }
        public DateTime? Return { get; set; }
    }
}
