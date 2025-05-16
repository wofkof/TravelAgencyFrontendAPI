namespace TravelAgency.Shared.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public ICollection<Employee> Employees { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
