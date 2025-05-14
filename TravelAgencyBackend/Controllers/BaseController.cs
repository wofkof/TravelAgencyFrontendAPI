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

        /// <summary>
        /// 權限檢查，不通過時回傳 Forbid()
        /// </summary>
        protected IActionResult? CheckPermissionOrForbid(string permissionName)
        {
            if (!_permissionService.HasPermission(permissionName))
                return Forbid($"您沒有「{permissionName}」的權限");
            return null;
        }

        /// <summary>
        /// 權限檢查（以布林值代表是否有權限），不通過時回傳 Forbid()
        /// </summary>
        protected IActionResult? CheckPermissionOrForbid(bool hasPermission, string displayName)
        {
            if (!hasPermission)
                return Forbid($"您沒有「{displayName}」的權限");
            return null;
        }

    }
}
