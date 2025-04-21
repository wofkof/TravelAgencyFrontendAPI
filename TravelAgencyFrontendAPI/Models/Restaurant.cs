namespace TravelAgencyFrontendAPI.Models
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public int DistrictId { get; set; }
        public string RestaurantName { get; set; }

        public District District { get; set; }
    }

}
