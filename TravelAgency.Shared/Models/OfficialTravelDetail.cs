﻿namespace TravelAgency.Shared.Models
{
    public enum DetailState
    {
        Locked,
        Deleted
    }
    public class OfficialTravelDetail
    {
        public int OfficialTravelDetailId { get; set; }
        public int OfficialTravelId { get; set; }

        public int? TravelNumber { get; set; }
        public decimal? AdultPrice { get; set; }
        public decimal? ChildPrice { get; set; }
        public decimal? BabyPrice { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public DetailState? State { get; set; }

        public OfficialTravel OfficialTravel { get; set; }

        public ICollection<GroupTravel> GroupTravels { get; set; } = new List<GroupTravel>();

        public ICollection<OfficialTravelSchedule> officialTravelSchedules { get; set; } = new List<OfficialTravelSchedule>();
    }

}
