using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels
{
    public class CallLogListViewModel
    {
        public List<CallLog> Logs { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public string? Keyword { get; set; }
        public Status? StatusFilter { get; set; }
    }

}
