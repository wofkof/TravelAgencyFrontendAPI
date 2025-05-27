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
    public class CompletedOrderDetailsController : Controller
    {
        private readonly AppDbContext _context;

        public CompletedOrderDetailsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CompletedOrderDetails
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.CompletedOrderDetails.Include(c => c.DocumentMenu).Include(c => c.OrderForm);
            return View(await appDbContext.ToListAsync());
        }

        // GET: CompletedOrderDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var completedOrderDetail = await _context.CompletedOrderDetails
                .Include(c => c.DocumentMenu)
                .Include(c => c.OrderForm)
                .FirstOrDefaultAsync(m => m.CompletedOrderDetailId == id);
            if (completedOrderDetail == null)
            {
                return NotFound();
            }

            return View(completedOrderDetail);
        }

        // GET: CompletedOrderDetails/Create
        public IActionResult Create()
        {
            ViewData["DocumentMenuId"] = new SelectList(_context.DocumentMenus, "MenuId", "MenuId");
            ViewData["OrderFormId"] = new SelectList(_context.OrderForms, "OrderId", "ChineseName");
            return View();
        }

        // POST: CompletedOrderDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CompletedOrderDetailId,DocumentMenuId,OrderFormId")] CompletedOrderDetail completedOrderDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(completedOrderDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DocumentMenuId"] = new SelectList(_context.DocumentMenus, "MenuId", "MenuId", completedOrderDetail.DocumentMenuId);
            ViewData["OrderFormId"] = new SelectList(_context.OrderForms, "OrderId", "ChineseName", completedOrderDetail.OrderFormId);
            return View(completedOrderDetail);
        }

        // GET: CompletedOrderDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var completedOrderDetail = await _context.CompletedOrderDetails.FindAsync(id);
            if (completedOrderDetail == null)
            {
                return NotFound();
            }
            ViewData["DocumentMenuId"] = new SelectList(_context.DocumentMenus, "MenuId", "MenuId", completedOrderDetail.DocumentMenuId);
            ViewData["OrderFormId"] = new SelectList(_context.OrderForms, "OrderId", "ChineseName", completedOrderDetail.OrderFormId);
            return View(completedOrderDetail);
        }

        // POST: CompletedOrderDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CompletedOrderDetailId,DocumentMenuId,OrderFormId")] CompletedOrderDetail completedOrderDetail)
        {
            if (id != completedOrderDetail.CompletedOrderDetailId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(completedOrderDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompletedOrderDetailExists(completedOrderDetail.CompletedOrderDetailId))
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
            ViewData["DocumentMenuId"] = new SelectList(_context.DocumentMenus, "MenuId", "MenuId", completedOrderDetail.DocumentMenuId);
            ViewData["OrderFormId"] = new SelectList(_context.OrderForms, "OrderId", "ChineseName", completedOrderDetail.OrderFormId);
            return View(completedOrderDetail);
        }

        // GET: CompletedOrderDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var completedOrderDetail = await _context.CompletedOrderDetails
                .Include(c => c.DocumentMenu)
                .Include(c => c.OrderForm)
                .FirstOrDefaultAsync(m => m.CompletedOrderDetailId == id);
            if (completedOrderDetail == null)
            {
                return NotFound();
            }

            return View(completedOrderDetail);
        }

        // POST: CompletedOrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var completedOrderDetail = await _context.CompletedOrderDetails.FindAsync(id);
            if (completedOrderDetail != null)
            {
                _context.CompletedOrderDetails.Remove(completedOrderDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompletedOrderDetailExists(int id)
        {
            return _context.CompletedOrderDetails.Any(e => e.CompletedOrderDetailId == id);
        }
    }
}
