using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class OfficialTravelFullDto
    {
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Cover { get; set; }

        public List<OfficialTravelDetailDto> Details { get; set; }
    }

    public class OfficialTravelDetailDto 
    {
        public int DetailId { get; set; }
        public int? Number { get; set; }
        public decimal? AdultPrice { get; set; }
        public decimal? ChildPrice { get; set; }
        public decimal? BabyPrice { get; set; }

        public List<GroupTravelDto> GroupTravels { get; set; }
        public List<ScheduleDto> Schedules { get; set; }
    }

    public class ScheduleDto 
    {
        public int ScheduleId { get; set; }
        public int Day { get; set; }
        public MealDto Meals { get; set; }
        public HotelDto? Hotel { get; set; }
        public List<AttractionDto> Attractions { get; set; }
    }

    public class GroupTravelDto
    {
        public int GroupId { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? TotalSeats { get; set; }
        public int? SoldSeats { get; set; }
        public string? GroupStatus { get; set; }
    }

    public class AttractionDto 
    {
        public int AttractionId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }

    }
    public class HotelDto 
    {
        public int AccommodationId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
    }

    public class MealDto
    {
        public string? Breakfast { get; set; }
        public string? Lunch { get; set; }
        public string? Dinner { get; set; }
    }
}
