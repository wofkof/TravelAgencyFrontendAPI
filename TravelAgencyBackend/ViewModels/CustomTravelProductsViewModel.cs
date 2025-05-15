using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels
{
    public class CustomTravelProductsViewModel
    {
        public IEnumerable<City> City { get; set; }
        public IEnumerable<District> District { get; set; }
        public IEnumerable<Attraction> Attraction { get; set; }
        public IEnumerable<Restaurant> Restaurant { get; set; }
        public IEnumerable<Accommodation> Hotel { get; set; }
        public IEnumerable<Transport> Transportation { get; set; }
             
    }
}
