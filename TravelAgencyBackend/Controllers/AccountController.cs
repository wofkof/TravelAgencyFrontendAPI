using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgencyBackend.Services;
using TravelAgencyBackend.ViewModels.Login;

namespace TravelAgencyBackend.Controllers
{
    public class AccountController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly PermissionCheckService _perm;

        public AccountController(AppDbContext context, PermissionCheckService perm)
            : base(perm)
        {
            _context = context;
            _perm = perm;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);

            }

            // 查詢員工資料，包含角色與該角色對應的權限
            var employee = await _context.Employees
                .Include(e => e.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(e => e.Phone == vm.Phone && e.Password == vm.Password);

            if (employee == null)
            {
                ModelState.AddModelError("", "電話或密碼錯誤");
                return View(vm);
            }

            // 儲存登入資訊到 Session
            HttpContext.Session.SetInt32("EmployeeId", employee.EmployeeId);
            HttpContext.Session.SetString("EmployeeName", employee.Name);
            HttpContext.Session.SetInt32("RoleId", employee.RoleId);
            HttpContext.Session.SetString("RoleName", employee.Role.RoleName);

            // 取出權限清單
            var permissionNames = employee.Role.RolePermissions
                .Select(rp => rp.Permission.PermissionName)
                .ToList();

            // 儲存權限清單到 Session（用逗號分隔）
            HttpContext.Session.SetString("Permissions", string.Join(",", permissionNames));

            // 依角色導向不同首頁（可選）
            // if (permissionNames.Contains("後台總管"))
            // {
            //     return RedirectToAction("AdminDashboard", "Home");
            // }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string phone, string email)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Phone == phone && e.Email == email);

            if (employee == null)
            {
                ModelState.AddModelError("", "找不到符合的帳號，請確認電話與信箱是否正確");
                return View();
            }

            TempData["EmployeeId"] = employee.EmployeeId;
            return RedirectToAction("ResetPassword");
        }

        public IActionResult ResetPassword()
        {
            if (TempData["EmployeeId"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            var vm = new ResetPasswordViewModel
            {
                EmployeeId = (int)TempData["EmployeeId"]
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var employee = await _context.Employees.FindAsync(vm.EmployeeId);
            if (employee == null) return NotFound();

            employee.Password = vm.NewPassword;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "密碼重設成功，請重新登入";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

    }
}
