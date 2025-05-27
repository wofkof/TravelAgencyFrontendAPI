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
    public class GrouptestController : Controller
    {
        private readonly AppDbContext _context;

        public GrouptestController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grouptest
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.GroupTravels.Include(g => g.OfficialTravelDetail);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Grouptest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupTravel = await _context.GroupTravels
                .Include(g => g.OfficialTravelDetail)
                .FirstOrDefaultAsync(m => m.GroupTravelId == id);
            if (groupTravel == null)
            {
                return NotFound();
            }

            return View(groupTravel);
        }

        // GET: Grouptest/Create
        public IActionResult Create()
        {
            ViewData["OfficialTravelDetailId"] = new SelectList(_context.OfficialTravelDetails, "OfficialTravelDetailId", "OfficialTravelDetailId");
            return View();
        }

        // POST: Grouptest/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupTravelId,OfficialTravelDetailId,DepartureDate,ReturnDate,TotalSeats,SoldSeats,OrderDeadline,MinimumParticipants,GroupStatus,CreatedAt,UpdatedAt,RecordStatus")] GroupTravel groupTravel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(groupTravel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OfficialTravelDetailId"] = new SelectList(_context.OfficialTravelDetails, "OfficialTravelDetailId", "OfficialTravelDetailId", groupTravel.OfficialTravelDetailId);
            return View(groupTravel);
        }

        // GET: Grouptest/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupTravel = await _context.GroupTravels.FindAsync(id);
            if (groupTravel == null)
            {
                return NotFound();
            }
            ViewData["OfficialTravelDetailId"] = new SelectList(_context.OfficialTravelDetails, "OfficialTravelDetailId", "OfficialTravelDetailId", groupTravel.OfficialTravelDetailId);
            return View(groupTravel);
        }

        // POST: Grouptest/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupTravelId,OfficialTravelDetailId,DepartureDate,ReturnDate,TotalSeats,SoldSeats,OrderDeadline,MinimumParticipants,GroupStatus,CreatedAt,UpdatedAt,RecordStatus")] GroupTravel groupTravel)
        {
            if (id != groupTravel.GroupTravelId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupTravel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupTravelExists(groupTravel.GroupTravelId))
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
            ViewData["OfficialTravelDetailId"] = new SelectList(_context.OfficialTravelDetails, "OfficialTravelDetailId", "OfficialTravelDetailId", groupTravel.OfficialTravelDetailId);
            return View(groupTravel);
        }

        // GET: Grouptest/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupTravel = await _context.GroupTravels
                .Include(g => g.OfficialTravelDetail)
                .FirstOrDefaultAsync(m => m.GroupTravelId == id);
            if (groupTravel == null)
            {
                return NotFound();
            }

            return View(groupTravel);
        }

        // POST: Grouptest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var groupTravel = await _context.GroupTravels.FindAsync(id);
            if (groupTravel != null)
            {
                _context.GroupTravels.Remove(groupTravel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupTravelExists(int id)
        {
            return _context.GroupTravels.Any(e => e.GroupTravelId == id);
        }
    }
}
