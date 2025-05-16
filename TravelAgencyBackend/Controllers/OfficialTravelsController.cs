using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.Services;
using TravelAgencyBackend.ViewModels;


namespace TravelAgencyBackend.Controllers
{
    public class OfficialTravelsController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly PermissionCheckService _perm;

        public OfficialTravelsController(AppDbContext context, PermissionCheckService perm)
            : base(perm)
        {
            _context = context;
            _perm = perm;
        }

        // GET: OfficialTravels
        public async Task<IActionResult> Index()
        {
            var check = CheckPermissionOrForbid("查看官方行程");
            if (check != null) return check;

            var appDbContext = _context.OfficialTravels.Include(o => o.CreatedByEmployee).Include(o => o.Region);

            return View(appDbContext);
        }

        // GET: OfficialTravels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var check = CheckPermissionOrForbid("查看官方行程");
            if (check != null) return check;

            if (id == null)
            {
                return NotFound();
            }

            var officialTravel = await _context.OfficialTravels
                .Include(o => o.CreatedByEmployee)
                .Include(o => o.Region)
                .FirstOrDefaultAsync(m => m.OfficialTravelId == id);
            if (officialTravel == null)
            {
                return NotFound();
            }

            return View(officialTravel);
        }

        // GET: OfficialTravels/Create
        public IActionResult Create()
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            ViewData["CreatedByEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "Name");
            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "Name");
            return View();
        }

        // POST: OfficialTravels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ViewModels.OfficialTravelViewModel model)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            if (ModelState.IsValid)
            {
                string? coverPath = null;

                if (model.CoverImage != null && model.CoverImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CoverImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImage.CopyToAsync(stream);
                    }

                    coverPath = "/uploads/covers/" + uniqueFileName;
                }

                var officialTravel = new OfficialTravel
                {
                    OfficialTravelId = model.OfficialTravelId,
                    CreatedByEmployeeId = model.CreatedByEmployeeId,
                    RegionId = model.RegionId,
                    Title = model.Title,
                    //ProjectYear = model.ProjectYear,
                    AvailableFrom = model.AvailableFrom,
                    AvailableUntil = model.AvailableUntil,
                    Description = model.Description,
                    Days = model.Days,
                    CoverPath = coverPath,
                    Status = (TravelAgency.Shared.Models.TravelStatus)model.Status,
                    CreatedAt = DateTime.Now
                };
                _context.Add(officialTravel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["CreatedByEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "Name", model.CreatedByEmployeeId);
            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "Name", model.RegionId);
            return View(model);

        }

        
        public async Task<IActionResult> Edit(int? id)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            if (id == null) return NotFound();

            var travel = await _context.OfficialTravels.FindAsync(id);
            if (travel == null) return NotFound();

            var vm = new OfficialTravelEditViewModel
            {
                OfficialTravelId = travel.OfficialTravelId,
                CreatedByEmployeeId = travel.CreatedByEmployeeId,
                RegionId = travel.RegionId,
                Title = travel.Title,
                //ProjectYear = travel.ProjectYear,
                AvailableFrom = (DateTime)travel.AvailableFrom,
                AvailableUntil = (DateTime)travel.AvailableUntil,
                Description = travel.Description,
                Days = (int)travel.Days,
                CoverPath = travel.CoverPath,
                CreatedAt = (DateTime)travel.CreatedAt,
                Status = (ViewModels.TravelStatus)travel.Status
            };

            ViewData["CreatedByEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "Name", travel.CreatedByEmployeeId);
            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "Country", travel.RegionId);

            return View(vm);
        }

        // POST: OfficialTravels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OfficialTravelEditViewModel vm)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            if (id != vm.OfficialTravelId) return NotFound();

            if (ModelState.IsValid)
            {
                var travelToUpdate = await _context.OfficialTravels.FindAsync(id);
                if (travelToUpdate == null) return NotFound();

                // 更新其他屬性
                travelToUpdate.Title = vm.Title;
                travelToUpdate.RegionId = vm.RegionId;
                travelToUpdate.CreatedByEmployeeId = vm.CreatedByEmployeeId;
                //travelToUpdate.ProjectYear = vm.ProjectYear;
                travelToUpdate.AvailableFrom = vm.AvailableFrom;
                travelToUpdate.AvailableUntil = vm.AvailableUntil;
                travelToUpdate.Description = vm.Description;
                travelToUpdate.Days = vm.Days;
                travelToUpdate.Status = (TravelAgency.Shared.Models.TravelStatus)vm.Status;
                travelToUpdate.UpdatedAt = DateTime.Now;

                // 處理封面圖片
                if (vm.Cover != null && vm.Cover.Length > 0)
                {
                    // 檢查並刪除舊的圖片
                    if (!string.IsNullOrEmpty(travelToUpdate.CoverPath))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", travelToUpdate.CoverPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);  // 刪除舊圖片
                        }
                    }

                    // 上傳新圖片
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.Cover.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await vm.Cover.CopyToAsync(stream);  // 儲存圖片
                    }

                    // 更新封面圖片路徑
                    travelToUpdate.CoverPath = "/uploads/covers/" + uniqueFileName;
                }

                // 儲存更新
                _context.Update(travelToUpdate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // 若ModelState無效，重新載入View，並提供選擇列表
            ViewData["CreatedByEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "Name", vm.CreatedByEmployeeId);
            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "Country", vm.RegionId);

            return View(vm);
        }



        // GET: OfficialTravels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            if (id == null)
            {
                return NotFound();
            }

            var officialTravel = await _context.OfficialTravels
                .Include(o => o.CreatedByEmployee)
                .Include(o => o.Region)
                .FirstOrDefaultAsync(m => m.OfficialTravelId == id);
            if (officialTravel == null)
            {
                return NotFound();
            }

            return View(officialTravel);
        }

        // POST: OfficialTravels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var officialTravel = await _context.OfficialTravels.FindAsync(id);
            if (officialTravel != null)
            {
                _context.OfficialTravels.Remove(officialTravel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OfficialTravelExists(int id)
        {
            return _context.OfficialTravels.Any(e => e.OfficialTravelId == id);
        }


        //private IEnumerable<SelectListItem> GetTravelStatusSelectList()
        //{
        //    return Enum.GetValues(typeof(TravelStatus))
        //        .Cast<TravelStatus>()
        //        .Select(e => new SelectListItem
        //        {
        //            Value = e.ToString(),
        //            Text = e.GetType()
        //                    .GetMember(e.ToString())
        //                    .First()
        //                    .GetCustomAttribute<DisplayAttribute>()?.Name ?? e.ToString()
        //        });
        //}

    }
}
