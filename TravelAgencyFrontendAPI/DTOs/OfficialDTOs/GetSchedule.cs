namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class GetSchedule
    {
        public int ScheduleId { get; set; }
        public int Day { get; set; }
        public string? Description { get; set; }
        public string? Breakfast { get; set; }
        public string? Lunch { get; set; }
        public string? Dinner { get; set; }
        public string? Hotel { get; set; }

        public int? Attraction1 { get; set; }
        public int? Attraction2 { get; set; }
        public int? Attraction3 { get; set; }
        public int? Attraction4 { get; set; }
        public int? Attraction5 { get; set; }
    }
}
