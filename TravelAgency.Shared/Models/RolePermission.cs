﻿namespace TravelAgency.Shared.Models
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Role Role { get; set; }
        public Permission Permission { get; set; }
    }

}
