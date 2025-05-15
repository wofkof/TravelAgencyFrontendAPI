using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels
{
    public class CustomTravelPendingViewModel
    {
        public IEnumerable<CustomTravel> CustomTravel { get; set; }
        public IEnumerable<CustomTravelContent> Content { get; set; }
        public IEnumerable<City> City { get; set; }
        public IEnumerable<District> District { get; set; }
        public IEnumerable<Attraction> Attraction { get; set; }
        public IEnumerable<Restaurant> Restaurant { get; set; }
        public IEnumerable<Accommodation> Hotel { get; set; }
        public IEnumerable<Transport> Transportation { get; set; }

        public CustomTravelContent NewContent { get; set; } = new CustomTravelContent();
        public CustomTravelContent EditContent { get; set; }
        public int SelectedCityId { get; set; }
        public int SelectedDistrictId { get; set; }
        
    }
}
