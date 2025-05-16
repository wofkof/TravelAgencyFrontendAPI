using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.Services;
using TravelAgencyBackend.ViewModels.Announcement;

namespace TravelAgencyBackend.Controllers
{
    public class AnnouncementController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly AppDbContext _context;
        private readonly PermissionCheckService _perm;
        public AnnouncementController(AppDbContext context, PermissionCheckService perm)
            :base(perm)
        {
            _context = context;
            _perm = perm;
        }
        public IActionResult List()
        {
            var check = CheckPermissionOrForbid("查看公告");
            if (check != null) return check;

            var data = _context.Announcements
                .Where(a => a.Status != AnnouncementStatus.Deleted)
                .Include(a => a.Employee)
                .Select(a => new AnnouncementViewModel
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content,
                    SentAt = a.SentAt,
                    Status = a.Status,
                    EmployeeName = a.Employee.Name
                }).ToList();

            return View(data);
        }


        [HttpGet]
        public IActionResult Create(int employeeId)
        {
            //ViewBag.Employees = new SelectList(
            // _context.Employees
            //.Where(e => e.Status == EmployeeStatus.Active),
            //"EmployeeId",
            //"Name"
            //);

            var check = CheckPermissionOrForbid("發布公告");
            if (check != null) return check;

            var employee = _context.Employees.Find(HttpContext.Session.GetInt32("EmployeeId"));
            if (HttpContext.Session.GetInt32("EmployeeId") == null) return RedirectToAction("Login", "Account");

            if (employee == null) return NotFound();

            var model = new AnnouncementViewModel { EmployeeId = employee.EmployeeId };
            ViewBag.Employees = employee.Name;
            return View(model);

        }

        [HttpPost]
        [HttpPost]
        public IActionResult Create(AnnouncementViewModel vm)
        {
            var check = CheckPermissionOrForbid("發布公告");
            if (check != null) return check;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null) return RedirectToAction("Login", "Account");

            var announcement = new Announcement
            {
                Title = vm.Title,
                Content = vm.Content,
                SentAt = vm.SentAt,
                Status = vm.Status,
                EmployeeId = employeeId.Value
            };

            _context.Announcements.Add(announcement);
            _context.SaveChanges();

            return RedirectToAction("List");
        }
        public IActionResult Details(int id)
        {
            var check = CheckPermissionOrForbid("查看公告");
            if (check != null) return check;

            var data = _context.Announcements
                .Include(a => a.Employee)
                .FirstOrDefault(a => a.AnnouncementId == id);

            if (data == null)
            {
                return NotFound();
            }

            var vm = new AnnouncementViewModel
            {
                AnnouncementId = data.AnnouncementId,
                Title = data.Title,
                Content = data.Content,
                SentAt = data.SentAt,
                Status = data.Status,
                EmployeeId = data.EmployeeId,
                EmployeeName = data.Employee.Name
            };

            return View(vm);
        }
        // GET: /Announcement/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var check = CheckPermissionOrForbid("發布公告");
            if (check != null) return check;

            var data = _context.Announcements
                .Include(a => a.Employee)
                .FirstOrDefault(a => a.AnnouncementId == id);

            if (data == null)
            {
                return NotFound();
            }

            var vm = new AnnouncementViewModel
            {
                AnnouncementId = data.AnnouncementId,
                Title = data.Title,
                Content = data.Content,
                SentAt = data.SentAt,
                Status = data.Status,
                EmployeeId = data.EmployeeId,
                EmployeeName = data.Employee.Name
            };

            ViewBag.Employees = new SelectList(_context.Employees, "EmployeeId", "Name", vm.EmployeeId);
            return View(vm);
        }

        // POST: /Announcement/Edit/5
        [HttpPost]
        public IActionResult Edit(int id, AnnouncementViewModel vm)
        {
            var check = CheckPermissionOrForbid("發布公告");
            if (check != null) return check;

            if (id != vm.AnnouncementId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Employees = new SelectList(_context.Employees, "EmployeeId", "Name", vm.EmployeeId);
                return View(vm);
            }

            var data = _context.Announcements.FirstOrDefault(a => a.AnnouncementId == id);
            if (data == null)
            {
                return NotFound();
            }

            data.Title = vm.Title;
            data.Content = vm.Content;
            data.SentAt = vm.SentAt;
            data.Status = vm.Status;
            data.EmployeeId = vm.EmployeeId;

            _context.SaveChanges();
            return RedirectToAction("List");
        }

        // GET: /Announcement/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var check = CheckPermissionOrForbid("發布公告");
            if (check != null) return check;

            var data = _context.Announcements
                .Include(a => a.Employee)
                .FirstOrDefault(a => a.AnnouncementId == id);

            if (data == null)
                return NotFound();

            var vm = new AnnouncementViewModel
            {
                AnnouncementId = data.AnnouncementId,
                Title = data.Title,
                Content = data.Content,
                SentAt = data.SentAt,
                Status = data.Status,
                EmployeeId = data.EmployeeId,
                EmployeeName = data.Employee.Name
            };

            return View(vm);
        }

        // POST: /Announcement/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var check = CheckPermissionOrForbid("發布公告");
            if (check != null) return check;

            var data = _context.Announcements.FirstOrDefault(a => a.AnnouncementId == id);
            if (data == null)
                return NotFound();

            data.Status = AnnouncementStatus.Deleted;
            _context.SaveChanges();

            return RedirectToAction("List");
        }

    }
}
