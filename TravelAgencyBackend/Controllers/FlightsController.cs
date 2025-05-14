using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgencyBackend.Services;

namespace TravelAgencyBackend.Controllers
{
    public class FlightsController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly PermissionCheckService _perm;

        public FlightsController(AppDbContext context, PermissionCheckService perm)
            : base(perm)
        {
            _context = context;
            _perm = perm;
        }

        // GET: Flights
        public async Task<IActionResult> Index()
        {
            var check = CheckPermissionOrForbid("查看官方行程");
            if (check != null) return check;

            return View(_context.Flights);
        }

        // GET: Flights/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var check = CheckPermissionOrForbid("查看官方行程");
            if (check != null) return check;

            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights
                .FirstOrDefaultAsync(m => m.FlightId == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // GET: Flights/Create
        public IActionResult Create()
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            return View();
        }

        // POST: Flights/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FlightId,AirlineCode,AirlineName,DepartureAirportCode,DepartureAirportName,ArrivalAirportCode,ArrivalAirportName,DepartureTime,ArrivalTime,Status,AircraftType,FlightUid,SyncedAt")] Flight flight)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            if (ModelState.IsValid)
            {
                _context.Add(flight);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(flight);
        }

        // GET: Flights/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }
            return View(flight);
        }

        // POST: Flights/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FlightId,AirlineCode,AirlineName,DepartureAirportCode,DepartureAirportName,ArrivalAirportCode,ArrivalAirportName,DepartureTime,ArrivalTime,Status,AircraftType,FlightUid,SyncedAt")] Flight flight)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            if (id != flight.FlightId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FlightExists(flight.FlightId))
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
            return View(flight);
        }

        // GET: Flights/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights
                .FirstOrDefaultAsync(m => m.FlightId == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // POST: Flights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var check = CheckPermissionOrForbid("管理官方行程");
            if (check != null) return check;

            var flight = await _context.Flights.FindAsync(id);
            if (flight != null)
            {
                _context.Flights.Remove(flight);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.FlightId == id);
        }
    }
}
