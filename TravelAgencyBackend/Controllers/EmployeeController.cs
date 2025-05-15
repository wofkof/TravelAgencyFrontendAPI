using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgencyBackend.ViewModels.Employee;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using TravelAgencyBackend.Helpers;
using Microsoft.AspNetCore.Hosting;
using TravelAgencyBackend.Services;
using TravelAgency.Shared.Models;



namespace TravelAgencyBackend.Controllers
{
    public class EmployeeController : BaseController
    {

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PermissionCheckService _perm;
        public EmployeeController(AppDbContext context, IWebHostEnvironment webHostEnvironment, PermissionCheckService perm)
            : base(perm)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _perm = perm;
        }


        //GET: Employees
        //public async Task<IActionResult> List(EmployeeKeyWordViewModel p)
        //{
        //    var query = _context.Employees
        //        .Where(e => e.Status != EmployeeStatus.Deleted)
        //        .Include(e => e.Role)
        //        .AsQueryable();

        //    if (!string.IsNullOrEmpty(p.txtKeyword))
        //    {
        //        query = query.Where(e => e.Name.Contains(p.txtKeyword) ||
        //                                 e.Email.Contains(p.txtKeyword) ||
        //                                 e.Phone.Contains(p.txtKeyword));
        //    }

        //    // 1️⃣ 先用匿名型別把原始資料查出來（不能用 GetDisplayName）
        //    var rawData = await query
        //        .Select(e => new
        //        {
        //            e.EmployeeId,
        //            e.Name,
        //            e.Gender,
        //            e.BirthDate,
        //            e.Phone,
        //            e.Email,
        //            e.Address,
        //            e.HireDate,
        //            e.Status,
        //            e.Note,
        //            RoleName = e.Role.RoleName
        //        }).ToListAsync();

        //    // 2️⃣ 再轉成 ViewModel，這時就可以安全使用 GetDisplayName()
        //    var result = rawData.Select(e => new EmployeeListViewModel
        //    {
        //        EmployeeId = e.EmployeeId,
        //        Name = e.Name,
        //        Gender = e.Gender,
        //        BirthDate = e.BirthDate,
        //        Phone = e.Phone,
        //        Email = e.Email,
        //        Address = e.Address,
        //        HireDate = e.HireDate,
        //        Status = e.Status, // ✅ 原 enum 型別
        //        Note = e.Note,
        //        RoleName = e.RoleName
        //    }).ToList();


        //    ViewBag.Keyword = p.txtKeyword;

        //    return View(result);
        //}

        public async Task<IActionResult> List(EmployeeKeyWordViewModel p, int page = 1)
        {
            var check = CheckPermissionOrForbid("查看員工");
            if (check != null) return check;

            int pageSize = 10;

            var query = _context.Employees
                .Where(e => e.Status != EmployeeStatus.Deleted)
                .Include(e => e.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(p.txtKeyword))
            {
                query = query.Where(e => e.Name.Contains(p.txtKeyword) ||
                                         e.Email.Contains(p.txtKeyword) ||
                                         e.Phone.Contains(p.txtKeyword));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var rawData = await query
                .OrderBy(e => e.EmployeeId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new
                {
                    e.EmployeeId,
                    e.Name,
                    e.Gender,
                    e.BirthDate,
                    e.Phone,
                    e.Email,
                    e.Address,
                    e.HireDate,
                    e.Status,
                    e.Note,
                    RoleName = e.Role.RoleName
                }).ToListAsync();

            var result = rawData.Select(e => new EmployeeListViewModel
            {
                EmployeeId = e.EmployeeId,
                Name = e.Name,
                Gender = e.Gender,
                BirthDate = (DateTime)e.BirthDate,
                Phone = e.Phone,
                Email = e.Email,
                Address = e.Address,
                HireDate = e.HireDate,
                Status = e.Status,
                Note = e.Note,
                RoleName = e.RoleName
            }).ToList();

            var vm = new EmployeeListPageViewModel
            {
                Employees = result,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize,
                Keyword = p.txtKeyword
            };

            return View(vm);
        }

        private void SetRole(object? selectRoleId = null) 
        {
            var allRoleNames = new[] { "業務人員", "客服人員", "內容管理員", "一般員工" };
            var allRoles = _context.Roles.Where(r => allRoleNames.Contains(r.RoleName)).ToList();

            ViewBag.RoleList = new SelectList(allRoles, "RoleId", "RoleName", selectRoleId);
            ViewBag.GenderList = EnumHelper.GetSelectListWithDisplayName<GenderType>();
            ViewBag.StatusList = EnumHelper.GetSelectListWithDisplayName<EmployeeStatus>(excludeDeleted: true);
        }

        public IActionResult Create()
        {
            var check = CheckPermissionOrForbid("管理員工");
            if (check != null) return check;

            SetRole();

            //ViewBag.RoleList = new SelectList(_context.Roles, "RoleId", "RoleName");
            //ViewBag.GenderList = EnumHelper.GetSelectListWithDisplayName<GenderType>();
            //ViewBag.StatusList = EnumHelper.GetSelectListWithDisplayName<EmployeeStatus>(excludeDeleted: true);


            var vm = new EmployeeCreateViewModel
            {
                BirthDate = new DateTime(1955, 1, 1),
                HireDate = DateTime.Now
            };

            return View(vm);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(EmployeeCreateViewModel vm)
        //{
        //    ViewBag.RoleList = new SelectList(_context.Roles, "RoleId", "RoleName", vm.RoleId);
        //    ViewBag.GenderList = EnumHelper.GetSelectListWithDisplayName<GenderType>();
        //    ViewBag.StatusList = EnumHelper.GetSelectListWithDisplayName<EmployeeStatus>(excludeDeleted: true);


        //    if (_context.Employees.Any(e => e.Email == vm.Email && e.Status != EmployeeStatus.Deleted))
        //    {
        //        ModelState.AddModelError("Email", "此信箱已被使用，請改用其他信箱。");
        //    }

        //    // ✅ 驗證 Phone 是否重複
        //    if (_context.Employees.Any(e => e.Phone == vm.Phone && e.Status != EmployeeStatus.Deleted))
        //    {
        //        ModelState.AddModelError("Phone", "此電話號碼已被使用，請再次確認輸入內容。");
        //    }

        //    // ❌ 有驗證錯誤就直接回原畫面
        //    if (!ModelState.IsValid)
        //    {
        //        return View(vm);
        //    }

        //    // ✅ 建立新員工實體
        //    var emp = new Employee
        //    {
        //        Name = vm.Name,
        //        Password = vm.Password,
        //        Email = vm.Email,
        //        Phone = vm.Phone,
        //        BirthDate = vm.BirthDate,
        //        HireDate = vm.HireDate,
        //        Gender = vm.Gender!.Value,
        //        Status = vm.Status!.Value,
        //        Address = vm.Address,
        //        Note = vm.Note,
        //        RoleId = vm.RoleId!.Value,

        //    };

        //    _context.Add(emp);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(List));
        //}

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeCreateViewModel vm)
        {
            var check = CheckPermissionOrForbid("管理員工");
            if (check != null) return check;

            if (_context.Employees.Any(m => m.Phone == vm.Phone))
                ModelState.AddModelError("Phone", "此手機已被註冊");

            if (!ModelState.IsValid) 
            {
                SetRole(vm.RoleId);
                return View(vm);
            }

            string? fileName = null;

            if (vm.Photo != null && vm.Photo.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.Photo.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.Photo.CopyToAsync(stream);
                }
            }

            //fileName ??= "default.png";
            fileName ??= "";
            var employee = new Employee
            {
                Name = vm.Name,
                Password = vm.Password,
                Email = vm.Email,
                Phone = vm.Phone,
                BirthDate = vm.BirthDate,
                HireDate = vm.HireDate,
                Gender = vm.Gender!.Value,
                Status = vm.Status!.Value,
                Address = vm.Address,
                Note = vm.Note,
                RoleId = vm.RoleId!.Value,
                ImagePath = fileName
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction("List");
        }


        public async Task<IActionResult> Edit(int? id)
        {
            var check = CheckPermissionOrForbid("管理員工");
            if (check != null) return check;

            SetRole();

            if (id == null) return NotFound();

            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            var vm = new EmployeeEditViewModel
            {   
                EmployeeId = emp.EmployeeId,
                Name = emp.Name,
                Email = emp.Email,
                Phone = emp.Phone,
                BirthDate = (DateTime)emp.BirthDate,
                HireDate = emp.HireDate,
                Gender = emp.Gender,
                Status = emp.Status,
                Address = emp.Address,
                Note = emp.Note,
                RoleId = emp.RoleId,
                ImagePath = emp.ImagePath

            };

            ViewBag.GenderList = Enum.GetValues(typeof(GenderType))
            .Cast<GenderType>()
            .Select(g => new SelectListItem
            {
                Text = g.GetType()
                .GetMember(g.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()?.Name ?? g.ToString(),
                Value = ((int)g).ToString()
            }).ToList();

            ViewBag.StatusList = Enum.GetValues(typeof(EmployeeStatus))
                .Cast<EmployeeStatus>()
                .Select(s => new SelectListItem
                {
                    Text = s.GetType()
                            .GetMember(s.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()?.Name ?? s.ToString(),
                    Value = ((int)s).ToString()
                }).ToList();

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeEditViewModel vm)
        {
            var check = CheckPermissionOrForbid("管理員工");
            if (check != null) return check;

            if (id != vm.EmployeeId) return NotFound();

            if (_context.Employees.Any(m => m.Phone == vm.Phone && m.EmployeeId != vm.EmployeeId ))
                ModelState.AddModelError("Phone", "此電話號碼已被使用。");
            
            if (!ModelState.IsValid)
            {
                // ❗這段是你目前少的
                SetRole(vm.RoleId);
                
                ViewBag.GenderList = Enum.GetValues(typeof(GenderType))
                    .Cast<GenderType>()
                    .Select(g => new SelectListItem
                    {
                        Text = g.GetType()
                            .GetMember(g.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()?.Name ?? g.ToString(),
                        Value = ((int)g).ToString()
                    }).ToList();

                ViewBag.StatusList = Enum.GetValues(typeof(EmployeeStatus))
                    .Cast<EmployeeStatus>()
                    .Select(s => new SelectListItem
                    {
                        Text = s.GetType()
                                .GetMember(s.ToString())
                                .First()
                                .GetCustomAttribute<DisplayAttribute>()?.Name ?? s.ToString(),
                        Value = ((int)s).ToString()
                    }).ToList();

                return View(vm);
            }

            var emp = await _context.Employees.FindAsync(vm.EmployeeId);
            if (emp == null) return NotFound();

            if (vm.Photo != null && vm.Photo.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                if (!string.IsNullOrEmpty(emp.ImagePath) && emp.ImagePath != "default.png")
                {
                    string oldPath = Path.Combine(uploadsFolder, emp.ImagePath);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.Photo.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.Photo.CopyToAsync(stream);
                }

                emp.ImagePath = fileName;
            }

            emp.Name = vm.Name;
            emp.Email = vm.Email;
            emp.Phone = vm.Phone;
            emp.BirthDate = vm.BirthDate;
            emp.HireDate = vm.HireDate;
            emp.Gender = vm.Gender;
            emp.Status = vm.Status;
            emp.Address = vm.Address;
            emp.Note = vm.Note;
            emp.RoleId = vm.RoleId;

            await _context.SaveChangesAsync();
            return RedirectToAction("List");
        }



        public async Task<IActionResult> Delete(int? id)
        {
            var check = CheckPermissionOrForbid("管理員工");
            if (check != null) return check;

            if (id == null) return NotFound();

            var emp = await _context.Employees
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (emp == null) return NotFound();

            var vm = new EmployeeDeleteViewModel
            {
                EmployeeId = emp.EmployeeId,
                Name = emp.Name,
                RoleName = emp.Role.RoleName,
                Phone = emp.Phone,
                Email = emp.Email,
                Note = emp.Note,
                ImagePath = emp.ImagePath

            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int EmployeeId)
        {
            var check = CheckPermissionOrForbid("管理員工");
            if (check != null) return check;

            var emp = await _context.Employees.FindAsync(EmployeeId);
            if (emp == null) return NotFound();

            // 軟刪除：只更改狀態，不移除資料
            emp.Status = EmployeeStatus.Deleted;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(List));
        }

        public IActionResult Details(int id)
        {
            var check = CheckPermissionOrForbid("管理員工");
            if (check != null) return check;

            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            var vm = new EmployeeDetailViewModel
            {
                EmployeeId = employee.EmployeeId,
                Name = employee.Name,
                Gender = employee.Gender.GetDisplayName(),
                BirthDate = (DateTime)employee.BirthDate,
                Phone = employee.Phone,
                Email = employee.Email,
                Address = employee.Address,
                HireDate = employee.HireDate,
                Status = employee.Status.GetDisplayName(),
                Note = employee.Note,
                ImagePath = employee.ImagePath

            };

            return View(vm);
        }


        /// 檢查指定 ID 的員工是否存在於資料庫中。
        /// Scaffold 預設產生的方法，未來可用於處理資料庫更新時的併發檢查（例如 Edit 或 Delete 操作）。
        /// 目前尚未使用，保留以供日後擴充用。
        //private bool EmployeeExists(int id)
        //{
        //    return _context.Employees.Any(e => e.EmployeeId == id);
        //}
    }

}
