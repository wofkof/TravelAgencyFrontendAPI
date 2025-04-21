using System.Data;

namespace TravelAgencyFrontendAPI.Models
{
    public enum EmployeeStatus
    {
        Active,
        Suspended,
        Deleted
    }
    public class Employee
    {
        public int EmployeeId { get; set; }
        public int RoleId { get; set; }

        public string Name { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public EmployeeStatus Status { get; set; }
        public string? Note { get; set; }

        public Role Role { get; set; }
    }
}
