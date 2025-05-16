namespace TravelAgencyBackend.ViewModels
{
    public class RolePermissionViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;

        public List<PermissionCheckboxItem> Permissions { get; set; } = new();
    }

    public class PermissionCheckboxItem
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = null!;
        public bool IsSelected { get; set; }
    }

}
