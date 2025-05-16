namespace TravelAgencyBackend.ViewModels.Employee
{
    public class EmployeeDetailViewModel
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = null!;
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public string Status { get; set; }
        public string? Note { get; set; }

        public string? ImagePath { get; set; }

    }
}
