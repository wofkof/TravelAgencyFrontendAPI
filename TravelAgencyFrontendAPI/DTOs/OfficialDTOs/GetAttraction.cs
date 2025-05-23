namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class GetAttraction
    {

        public int AttractionId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
    }
}
