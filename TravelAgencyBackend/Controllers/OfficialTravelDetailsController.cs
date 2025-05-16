//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using TravelAgency.Shared.Data;
//using TravelAgency.Shared.Models;
//using TravelAgencyBackend.Services;

//namespace TravelAgencyBackend.Controllers
//{
//    public class OfficialTravelDetailsController : BaseController
//    {
//        private readonly AppDbContext _context;
//        private readonly PermissionCheckService _perm;
//        public OfficialTravelDetailsController(AppDbContext context, PermissionCheckService perm)
//            : base(perm)
//        {
//            _context = context;
//            _perm = perm;
//        }

//        // GET: OfficialTravelDetails
//        public async Task<IActionResult> Index()
//        {
//            var check = CheckPermissionOrForbid("查看官方行程");
//            if (check != null) return check;
//            // 楷茵
//            //var appDbContext = _context.OfficialTravelDetails.Include(o => o.Flight).Include(o => o.OfficialTravel);
//            return View(
//                //appDbContext
//                );
//        }

//        // GET: OfficialTravelDetails/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            var check = CheckPermissionOrForbid("查看官方行程");
//            if (check != null) return check;

//            if (id == null)
//            {
//                return NotFound();
//            }

//            var officialTravelDetail = await _context.OfficialTravelDetails
//                // 楷茵
//                //.Include(o => o.Flight)
//                .Include(o => o.OfficialTravel)
//                .FirstOrDefaultAsync(m => m.OfficialTravelDetailId == id);
//            if (officialTravelDetail == null)
//            {
//                return NotFound();
//            }

//            return View(officialTravelDetail);
//        }

//        // GET: OfficialTravelDetails/Create
//        public IActionResult Create()
//        {
//            var check = CheckPermissionOrForbid("管理官方行程");
//            if (check != null) return check;
//            // 楷茵
//            //ViewData["FlightId"] = new SelectList(_context.Flights, "FlightId", "AirlineCode");
//            ViewData["OfficialTravelId"] = new SelectList(_context.OfficialTravels, "OfficialTravelId", "Title");
//            return View();
//        }

//        // POST: OfficialTravelDetails/Create
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("OfficialTravelDetailId,OfficialTravelId,FlightId,DepartureDate,ReturnDate,Price,Seats,SoldSeats,MinimumGroupSize,BookingDeadline,GroupStatus,CreatedAt,UpdatedAt")] OfficialTravelDetail officialTravelDetail)
//        {
//            var check = CheckPermissionOrForbid("管理官方行程");
//            if (check != null) return check;

//            ModelState.Remove("OfficialTravel");
//            ModelState.Remove("Flight");
//            if (ModelState.IsValid)
//            {
//                _context.Add(officialTravelDetail);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            // 楷茵
//            //ViewData["FlightId"] = new SelectList(_context.Flights, "FlightId", "AirlineCode", officialTravelDetail.FlightId);
//            ViewData["OfficialTravelId"] = new SelectList(_context.OfficialTravels, "OfficialTravelId", "Title", officialTravelDetail.OfficialTravelId);
//            return View(officialTravelDetail);
//        }

//        // GET: OfficialTravelDetails/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            var check = CheckPermissionOrForbid("管理官方行程");
//            if (check != null) return check;

//            if (id == null)
//            {
//                return NotFound();
//            }

//            var officialTravelDetail = await _context.OfficialTravelDetails.FindAsync(id);
//            if (officialTravelDetail == null)
//            {
//                return NotFound();
//            }
//            // 楷茵
//            //ViewData["FlightId"] = new SelectList(_context.Flights, "FlightId", "AirlineCode", officialTravelDetail.FlightId);
//            ViewData["OfficialTravelId"] = new SelectList(_context.OfficialTravels, "OfficialTravelId", "Title", officialTravelDetail.OfficialTravelId);
//            return View(officialTravelDetail);
//        }

//        // POST: OfficialTravelDetails/Edit/5
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("OfficialTravelDetailId,OfficialTravelId,FlightId,DepartureDate,ReturnDate,Price,Seats,SoldSeats,MinimumGroupSize,BookingDeadline,GroupStatus,CreatedAt,UpdatedAt")] OfficialTravelDetail officialTravelDetail)
//        {
//            var check = CheckPermissionOrForbid("管理官方行程");
//            if (check != null) return check;

//            ModelState.Remove("OfficialTravel");
//            ModelState.Remove("Flight");
//            if (id != officialTravelDetail.OfficialTravelDetailId)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    _context.Update(officialTravelDetail);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!OfficialTravelDetailExists(officialTravelDetail.OfficialTravelDetailId))
//                    {
//                        return NotFound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return RedirectToAction(nameof(Index));
//            }
//            // 楷茵
//            //ViewData["FlightId"] = new SelectList(_context.Flights, "FlightId", "AirlineCode", officialTravelDetail.FlightId);
//            ViewData["OfficialTravelId"] = new SelectList(_context.OfficialTravels, "OfficialTravelId", "Title", officialTravelDetail.OfficialTravelId);
//            return View(officialTravelDetail);
//        }

//        // GET: OfficialTravelDetails/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            var check = CheckPermissionOrForbid("管理官方行程");
//            if (check != null) return check;

//            if (id == null)
//            {
//                return NotFound();
//            }

//            var officialTravelDetail = await _context.OfficialTravelDetails
//                // 楷茵
//                //.Include(o => o.Flight)
//                .Include(o => o.OfficialTravel)
//                .FirstOrDefaultAsync(m => m.OfficialTravelDetailId == id);
//            if (officialTravelDetail == null)
//            {
//                return NotFound();
//            }

//            return View(officialTravelDetail);
//        }

//        // POST: OfficialTravelDetails/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var check = CheckPermissionOrForbid("管理官方行程");
//            if (check != null) return check;

//            var officialTravelDetail = await _context.OfficialTravelDetails.FindAsync(id);
//            if (officialTravelDetail != null)
//            {
//                _context.OfficialTravelDetails.Remove(officialTravelDetail);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool OfficialTravelDetailExists(int id)
//        {
//            return _context.OfficialTravelDetails.Any(e => e.OfficialTravelDetailId == id);
//        }
//    }
//}
