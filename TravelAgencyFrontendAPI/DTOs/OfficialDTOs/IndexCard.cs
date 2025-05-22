namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class IndexCard:baseDTO
    {
        public decimal? Price { get; set; }
        public int DetailId { get; set; }
        public int GroupId { get; set; }

        public DateTime? DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? Days { get; set; }
    }
}
