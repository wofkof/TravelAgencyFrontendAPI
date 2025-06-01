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
    public class OfficialTravelDetailsController : Controller
    {
        private readonly AppDbContext _context;

        public OfficialTravelDetailsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OfficialTravelDetails
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.OfficialTravelDetails.Include(o => o.OfficialTravel);
            return View(await appDbContext.ToListAsync());
        }

        // GET: OfficialTravelDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var officialTravelDetail = await _context.OfficialTravelDetails
                .Include(o => o.OfficialTravel)
                .FirstOrDefaultAsync(m => m.OfficialTravelDetailId == id);
            if (officialTravelDetail == null)
            {
                return NotFound();
            }

            return View(officialTravelDetail);
        }

        // GET: OfficialTravelDetails/Create
        public IActionResult Create()
        {
            ViewData["OfficialTravelId"] = new SelectList(_context.OfficialTravels, "OfficialTravelId", "Title");
            return View();
        }

        // POST: OfficialTravelDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OfficialTravelDetailId,OfficialTravelId,TravelNumber,AdultPrice,ChildPrice,BabyPrice,UpdatedAt,State")] OfficialTravelDetail officialTravelDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(officialTravelDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OfficialTravelId"] = new SelectList(_context.OfficialTravels, "OfficialTravelId", "Title", officialTravelDetail.OfficialTravelId);
            return View(officialTravelDetail);
        }

        // GET: OfficialTravelDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var officialTravelDetail = await _context.OfficialTravelDetails.FindAsync(id);
            if (officialTravelDetail == null)
            {
                return NotFound();
            }
            ViewData["OfficialTravelId"] = new SelectList(_context.OfficialTravels, "OfficialTravelId", "Title", officialTravelDetail.OfficialTravelId);
            return View(officialTravelDetail);
        }

        // POST: OfficialTravelDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OfficialTravelDetailId,OfficialTravelId,TravelNumber,AdultPrice,ChildPrice,BabyPrice,UpdatedAt,State")] OfficialTravelDetail officialTravelDetail)
        {
            if (id != officialTravelDetail.OfficialTravelDetailId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(officialTravelDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OfficialTravelDetailExists(officialTravelDetail.OfficialTravelDetailId))
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
            ViewData["OfficialTravelId"] = new SelectList(_context.OfficialTravels, "OfficialTravelId", "Title", officialTravelDetail.OfficialTravelId);
            return View(officialTravelDetail);
        }

        // GET: OfficialTravelDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var officialTravelDetail = await _context.OfficialTravelDetails
                .Include(o => o.OfficialTravel)
                .FirstOrDefaultAsync(m => m.OfficialTravelDetailId == id);
            if (officialTravelDetail == null)
            {
                return NotFound();
            }

            return View(officialTravelDetail);
        }

        // POST: OfficialTravelDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var officialTravelDetail = await _context.OfficialTravelDetails.FindAsync(id);
            if (officialTravelDetail != null)
            {
                _context.OfficialTravelDetails.Remove(officialTravelDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OfficialTravelDetailExists(int id)
        {
            return _context.OfficialTravelDetails.Any(e => e.OfficialTravelDetailId == id);
        }
    }
}
