namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class TravelDetailResultDto
    {
        public int ProjectId { get; set; }
        public int DetailId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Cover { get; set; }
        public int? Number { get; set; }
        public decimal? AdultPrice { get; set; }
        public int GroupTravelId { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? AvailableSeats { get; set; }
        public int? TotalSeats { get; set; }
        public List<GroupTravelDto> GroupTravels { get; set; }
        public List<TravelScheduleDto> Schedules { get; set; }
    }

    public class TravelScheduleDto
    {
        public int ScheduleId { get; set; }
        public int Day { get; set; }
        public string ScheduleDescription { get; set; }
        public string Breakfast { get; set; }
        public string Lunch { get; set; }
        public string Dinner { get; set; }
        public string Hotel { get; set; }
        public int? Attraction1 { get; set; }
        public int? Attraction2 { get; set; }
        public int? Attraction3 { get; set; }
        public int? Attraction4 { get; set; }
        public int? Attraction5 { get; set; }
    }

    public class GroupTravelDto
    {
        public int GroupTravelId { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? AvailableSeats { get; set; }
        public int? TotalSeats { get; set; }
        public decimal? Price { get; set; }
        public string? StatusText { get; set; } // e.g., "可成行", "尚未成團"
    }

}
