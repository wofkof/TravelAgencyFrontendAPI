using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.Extensions;


namespace TravelAgencyFrontendAPI.DTOs.CustomTravelDTOs
{    
    public class CustomTravelContentDto
    {
        public int CustomTravelId { get; set; }

        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public TravelItemCategory Category { get; set; }
        public string CategoryText => Category.TurnChinese();

        public int Day { get; set; }
        public string Time { get; set; }
        public string? AccommodationName { get; set; }
    }
}
