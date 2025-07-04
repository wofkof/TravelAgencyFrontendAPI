﻿namespace TravelAgencyBackend.ViewModels.Employee
{
    public class EmployeeListPageViewModel
    {
        public IEnumerable<EmployeeListViewModel> Employees { get; set; } = new List<EmployeeListViewModel>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string? Keyword { get; set; }

        public int TotalCount { get; set; }    
        public int PageSize { get; set; }       

    }
}
