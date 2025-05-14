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
    public class TravelSuppliersController : Controller
    {
        private readonly AppDbContext _context;

        public TravelSuppliersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: TravelSuppliers
        public async Task<IActionResult> Index()
        {
            return View(_context.TravelSuppliers);
        }

        // GET: TravelSuppliers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelSupplier = await _context.TravelSuppliers
                .FirstOrDefaultAsync(m => m.TravelSupplierId == id);
            if (travelSupplier == null)
            {
                return NotFound();
            }

            return View(travelSupplier);
        }

        // GET: TravelSuppliers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TravelSuppliers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TravelSupplierId,SupplierName,SupplierType,ContactName,ContactPhone,ContactEmail,Note")] TravelSupplier travelSupplier)
        {
            if (ModelState.IsValid)
            {
                _context.Add(travelSupplier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(travelSupplier);
        }

        // GET: TravelSuppliers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelSupplier = await _context.TravelSuppliers.FindAsync(id);
            if (travelSupplier == null)
            {
                return NotFound();
            }
            return View(travelSupplier);
        }

        // POST: TravelSuppliers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TravelSupplierId,SupplierName,SupplierType,ContactName,ContactPhone,ContactEmail,Note")] TravelSupplier travelSupplier)
        {
            if (id != travelSupplier.TravelSupplierId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(travelSupplier);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TravelSupplierExists(travelSupplier.TravelSupplierId))
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
            return View(travelSupplier);
        }

        // GET: TravelSuppliers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelSupplier = await _context.TravelSuppliers
                .FirstOrDefaultAsync(m => m.TravelSupplierId == id);
            if (travelSupplier == null)
            {
                return NotFound();
            }

            return View(travelSupplier);
        }

        // POST: TravelSuppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var travelSupplier = await _context.TravelSuppliers.FindAsync(id);
            if (travelSupplier != null)
            {
                _context.TravelSuppliers.Remove(travelSupplier);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TravelSupplierExists(int id)
        {
            return _context.TravelSuppliers.Any(e => e.TravelSupplierId == id);
        }
    }
}
