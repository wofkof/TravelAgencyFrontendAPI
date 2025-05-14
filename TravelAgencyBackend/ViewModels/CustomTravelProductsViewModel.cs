using TravelAgencyBackend.Models;

namespace TravelAgencyBackend.ViewModels
{
    public class CustomTravelProductsViewModel
    {
        public IEnumerable<City> City { get; set; }
        public IEnumerable<District> District { get; set; }
        public IEnumerable<Attraction> Attraction { get; set; }
        public IEnumerable<Restaurant> Restaurant { get; set; }
        public IEnumerable<Hotel> Hotel { get; set; }
        public IEnumerable<Transportation> Transportation { get; set; }
             
    }
}
