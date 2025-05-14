using System;
using System.Collections.Generic;
using System.Linq;
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
    public class OfficialTravelSchedulesController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly PermissionCheckService _perm;

        public OfficialTravelSchedulesController(AppDbContext context, PermissionCheckService perm)
            : base(perm)
        {
            _context = context;
            _perm = perm;
        }

        // GET: OfficialTravelSchedules
        public async Task<IActionResult> Index()
        {
            var check = CheckPermissionOrForbid("查看官方行程");
            if (check != null) return check;

            var appDbContext = _context.OfficialTravelSchedules
                .Include(o => o.OfficialTravelDetail)
                .ThenInclude(t => t.OfficialTravel);
            return View(appDbContext);
        }
        //public IActionResult Index()
        //{
        //    var schedules = _context.OfficialTravelSchedules
        //        .Include(s => s.OfficialTravelDetail)
        //            .ThenInclude(d => d.OfficialTravel)
        //        .ToList();

        //    var hotels = _context.Hotels.ToDictionary(h => h.HotelId, h => h.HotelName);
        //    var attractions = _context.Attractions.ToDictionary(a => a.AttractionId, a => a.ScenicSpotName);
        //    var restaurants = _context.Restaurants.ToDictionary(r => r.RestaurantId, r => r.RestaurantName);

        //    var viewModel = schedules.Select(s =>
        //    {
        //        string activityName = s.Category switch
        //        {
        //            TravelActivityType.Hotel => hotels.ContainsKey(s.ItemId) ? hotels[s.ItemId] : "查無飯店",
        //            TravelActivityType.Attraction => attractions.ContainsKey(s.ItemId) ? attractions[s.ItemId] : "查無景點",
        //            TravelActivityType.Restaurant => restaurants.ContainsKey(s.ItemId) ? restaurants[s.ItemId] : "查無餐廳",
        //            _ => "未知類型"
        //        };

        //        return new ScheduleWithActivityNameViewModel
        //        {
        //            Schedule = s,
        //            ActivityName = activityName
        //        };
        //    }).ToList();

        //    return View(viewModel);
        //}

        // GET: OfficialTravelSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var check = CheckPermissionOrForbid("查看官方行程");
            if (check != null) return check;

            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.OfficialTravelSchedules
                .Include(o => o.OfficialTravelDetail)
                .FirstOrDefaultAsync(m => m.OfficialTravelScheduleId == id);

            if (schedule == null)
            {
                return NotFound();
            }

            // 根據活動類型查詢對應的資料表
            // 楷茵
            string activityName = schedule.Category switch
            {
                TravelActivityType.Hotel => await _context.Hotels
                    .Where(h => h.HotelId == schedule.ItemId)
                    .Select(h => h.HotelName)
                    .FirstOrDefaultAsync() ?? "",

                TravelActivityType.Attraction => await _context.Attractions
                    .Where(a => a.AttractionId == schedule.ItemId)
                    .Select(a => a.ScenicSpotName)
                    .FirstOrDefaultAsync() ?? "",

                TravelActivityType.Restaurant => await _context.Restaurants
                    .Where(r => r.RestaurantId == schedule.ItemId)
                    .Select(r => r.RestaurantName)
                    .FirstOrDefaultAsync() ?? "",

                _ => ""
            };

            // 建立 ViewModel
            var viewModel = new ScheduleWithActivityNameViewModel
            {
                Schedule = schedule,
                ActivityName = activityName
            };

            return View(viewModel);
        }


        // GET: OfficialTravelSchedules/Create
        public IActionResult Create()
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            ViewData["OfficialTravelDetailId"] = new SelectList(_context.OfficialTravelDetails, "OfficialTravelDetailId", "OfficialTravelDetailId");
            return View();
        }

        // POST: OfficialTravelSchedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OfficialTravelScheduleId,OfficialTravelDetailId,ItemId,Category,Day,StartTime,Date,Description,Note1,Note2")] OfficialTravelSchedule officialTravelSchedule)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            if (ModelState.IsValid)
            {
                _context.Add(officialTravelSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }
            ViewData["OfficialTravelDetailId"] = new SelectList(_context.OfficialTravelDetails, "OfficialTravelDetailId", "OfficialTravelDetailId", officialTravelSchedule.OfficialTravelDetailId);
            return View(officialTravelSchedule);
        }

        // GET: OfficialTravelSchedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            if (id == null)
            {
                return NotFound();
            }

            var officialTravelSchedule = await _context.OfficialTravelSchedules.FindAsync(id);
            if (officialTravelSchedule == null)
            {
                return NotFound();
            }
            ViewData["OfficialTravelDetailId"] = new SelectList(_context.OfficialTravelDetails, "OfficialTravelDetailId", "OfficialTravelDetailId", officialTravelSchedule.OfficialTravelDetailId);
            return View(officialTravelSchedule);
        }

        // POST: OfficialTravelSchedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OfficialTravelScheduleId,OfficialTravelDetailId,ItemId,Category,Day,StartTime,Date,Description,Note1,Note2")] OfficialTravelSchedule officialTravelSchedule)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            if (id != officialTravelSchedule.OfficialTravelScheduleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(officialTravelSchedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OfficialTravelScheduleExists(officialTravelSchedule.OfficialTravelScheduleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["OfficialTravelDetailId"] = new SelectList(_context.OfficialTravelDetails, "OfficialTravelDetailId", "OfficialTravelDetailId", officialTravelSchedule.OfficialTravelDetailId);
            return View(officialTravelSchedule);
        }

        // GET: OfficialTravelSchedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            if (id == null)
            {
                return NotFound();
            }

            var officialTravelSchedule = await _context.OfficialTravelSchedules
                .Include(o => o.OfficialTravelDetail)
                .FirstOrDefaultAsync(m => m.OfficialTravelScheduleId == id);
            if (officialTravelSchedule == null)
            {
                return NotFound();
            }

            return View(officialTravelSchedule);
        }

        // POST: OfficialTravelSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            var officialTravelSchedule = await _context.OfficialTravelSchedules.FindAsync(id);
            if (officialTravelSchedule != null)
            {
                _context.OfficialTravelSchedules.Remove(officialTravelSchedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OfficialTravelScheduleExists(int id)
        {
            return _context.OfficialTravelSchedules.Any(e => e.OfficialTravelScheduleId == id);
        }
    }
}
