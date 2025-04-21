namespace TravelAgencyFrontendAPI.Models
{
    public class Accommodation
    {
        public int AccommodationId { get; set; }
        public int DistrictId { get; set; }
        public string AccommodationName { get; set; }

        public District District { get; set; }
    }

}
