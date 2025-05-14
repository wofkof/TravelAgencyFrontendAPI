using TravelAgencyBackend.Models;

namespace TravelAgencyBackend.ViewModels.Employee
{
    public class EmployeeIndexViewModel
    {
        public string? SearchText { get; set; }
        public EmployeeStatus? FilterStatus { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }

        public List<EmployeeListViewModel> Employees { get; set; } = new();

    }
}
