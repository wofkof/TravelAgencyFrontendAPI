namespace TravelAgency.Shared.Models
{
    public class Permission
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string? Caption { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
