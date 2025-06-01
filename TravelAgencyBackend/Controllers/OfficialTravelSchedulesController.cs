using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.Controllers
{
    public class OfficialTravelSchedulesController : Controller
    {
        private readonly AppDbContext _context;

        public OfficialTravelSchedulesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OfficialTravelSchedules
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.OfficialTravelSchedules.Include(o => o.OfficialTravelDetail);
            return View(await appDbContext.ToListAsync());
        }

        // GET: OfficialTravelSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
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

        // GET: OfficialTravelSchedules/Create
        public IActionResult Create()
        {
            ViewData["OfficialTravelDetailId"] = new SelectList(_context.OfficialTravelDetails, "OfficialTravelDetailId", "OfficialTravelDetailId");
            return View();
        }

        // POST: OfficialTravelSchedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OfficialTravelScheduleId,OfficialTravelDetailId,Day,Description,Breakfast,Lunch,Dinner,Hotel,Attraction1,Attraction2,Attraction3,Attraction4,Attraction5,Note1,Note2")] OfficialTravelSchedule officialTravelSchedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(officialTravelSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OfficialTravelDetailId"] = new SelectList(_context.OfficialTravelDetails, "OfficialTravelDetailId", "OfficialTravelDetailId", officialTravelSchedule.OfficialTravelDetailId);
            return View(officialTravelSchedule);
        }

        // GET: OfficialTravelSchedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
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
        public async Task<IActionResult> Edit(int id, [Bind("OfficialTravelScheduleId,OfficialTravelDetailId,Day,Description,Breakfast,Lunch,Dinner,Hotel,Attraction1,Attraction2,Attraction3,Attraction4,Attraction5,Note1,Note2")] OfficialTravelSchedule officialTravelSchedule)
        {
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
