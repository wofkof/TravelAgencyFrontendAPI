using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.DTOs.CustomTravelDTOs
{
    public class ContentInputDto
    {
        public int CustomTravelId { get; set; }

        public int ItemId { get; set; }
        public TravelItemCategory Category { get; set; }

        public int Day { get; set; }
        public string Time { get; set; }
        public string? AccommodationName { get; set; }
    }
}
