namespace TravelAgencyFrontendAPI.Models
{
    public class Attraction
    {
        public int AttractionId { get; set; }
        public int DistrictId { get; set; }
        public string AttractionName { get; set; }

        public District District { get; set; }
    }

}
