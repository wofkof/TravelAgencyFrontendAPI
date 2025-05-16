using Microsoft.AspNetCore.Mvc;
using TravelAgencyBackend.Services;

namespace TravelAgencyBackend.Controllers
{
    public class BaseController : Controller
    {
        protected readonly PermissionCheckService _permissionService;

        public BaseController(PermissionCheckService permissionService)
        {
            _permissionService = permissionService;
        }
        
        protected IActionResult? CheckPermissionOrForbid(string permissionName)
        {
            if (!_permissionService.HasPermission(permissionName))
                return Forbid($"您沒有「{permissionName}」的權限");
            return null;
        }

        protected IActionResult? CheckPermissionOrForbid(bool hasPermission, string displayName)
        {
            if (!hasPermission)
                return Forbid($"您沒有「{displayName}」的權限");
            return null;
        }

        protected int GetCurrentEmployeeId()
        {
            return HttpContext.Session.GetInt32("EmployeeId") ?? 0;
        }

    }
}
