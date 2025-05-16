using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels
{
    public class ScheduleWithActivityNameViewModel
    {
        public OfficialTravelSchedule Schedule { get; set; }
        public string ActivityName { get; set; } = "";
    }

}
