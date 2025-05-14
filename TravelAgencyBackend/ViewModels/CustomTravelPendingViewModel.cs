using TravelAgencyBackend.Models;

namespace TravelAgencyBackend.ViewModels
{
    public class CustomTravelPendingViewModel
    {
        public IEnumerable<CustomTravel> CustomTravel { get; set; }
        public IEnumerable<Content> Content { get; set; }
        public IEnumerable<City> City { get; set; }
        public IEnumerable<District> District { get; set; }
        public IEnumerable<Attraction> Attraction { get; set; }
        public IEnumerable<Restaurant> Restaurant { get; set; }
        public IEnumerable<Hotel> Hotel { get; set; }
        public IEnumerable<Transportation> Transportation { get; set; }

        public Content NewContent { get; set; } = new Content();
        public Content EditContent { get; set; }
        public int SelectedCityId { get; set; }
        public int SelectedDistrictId { get; set; }
        
    }
}
