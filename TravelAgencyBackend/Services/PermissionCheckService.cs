using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;

namespace TravelAgencyBackend.Services
{
    public class PermissionCheckService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public PermissionCheckService(AppDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        //private int? CurrentEmployeeId
        //{
        //    get => _httpContext.HttpContext?.Session.GetInt32("EmployeeId");
        //    set
        //    {
        //        if (_httpContext.HttpContext != null)
        //        {
        //            _httpContext.HttpContext.Session.SetInt32("EmployeeId", value ?? 0);
        //        }
        //    }
        //}

        //public bool HasPermission(string permissionName)
        //{
        //    var employee = _context.Employees
        //        .Include(e => e.Role)
        //            .ThenInclude(r => r.RolePermissions)
        //            .ThenInclude(rp => rp.Permission)
        //        .FirstOrDefault(e => e.EmployeeId == CurrentEmployeeId);

        //    return employee?.Role.RolePermissions
        //        .Any(rp => rp.Permission.PermissionName == permissionName) ?? false;
        //}
        private Dictionary<string, bool>? _permissionCache;


        private int? CurrentEmployeeId =>
        _httpContext.HttpContext?.Session.GetInt32("EmployeeId");

        // 查詢一次所有權限並快取
        private void EnsurePermissionsLoaded()
        {
            if (_permissionCache != null) return;

            var permissions = _context.Employees
                .AsNoTracking() // 提升效能且避免追蹤
                .Include(e => e.Role)
                    .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefault(e => e.EmployeeId == CurrentEmployeeId)?
                .Role.RolePermissions
                .Select(rp => rp.Permission.PermissionName)
                .ToList();

            _permissionCache = permissions?
                .Distinct()
                .ToDictionary(name => name, _ => true) ?? new();
        }

        public bool HasPermission(string permissionName)
        {
            EnsurePermissionsLoaded();
            return _permissionCache!.ContainsKey(permissionName);
        }

        // 權限屬性
        public bool CanViewMembers => HasPermission("查看會員");
        public bool CanManageMembers => HasPermission("管理會員");
        public bool CanEditMemberPassword => HasPermission("修改會員密碼");
        public bool CanViewParticipants => HasPermission("查看參與人");
        public bool CanManageParticipants => HasPermission("管理參與人");
        public bool CanViewEmployees => HasPermission("查看員工");
        public bool CanManageEmployees => HasPermission("管理員工");
        public bool CanManageRoles => HasPermission("管理角色");
        public bool CanSetRoles => HasPermission("設定角色權限");
        public bool CanManageChatRooms => HasPermission("管理聊天室");
        public bool CanViewAnnouncements => HasPermission("查看公告");
        public bool CanPushAnnouncements => HasPermission("發布公告");
        public bool CanManagePermissions => HasPermission("管理權限");
        public bool CanViewCustomTravels => HasPermission("查看客製化行程");
        public bool CanManageCustomTravels => HasPermission("管理客製化行程");
        public bool CanViewOfficialTravels => HasPermission("查看官方行程");
        public bool CanManageOfficialTravels => HasPermission("管理官方行程");
        public bool CanViewOrders => HasPermission("查看訂單");
        public bool CanManageOrders => HasPermission("管理訂單");
        public bool CanViewHome => HasPermission("查看首頁");
        public bool CanViewCarts => HasPermission("查看購物車");
        public bool CanManageCarts => HasPermission("管理購物車");
    }
}