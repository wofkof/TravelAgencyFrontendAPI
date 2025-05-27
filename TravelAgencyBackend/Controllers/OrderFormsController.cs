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
    public class OrderFormsController : Controller
    {
        private readonly AppDbContext _context;

        public OrderFormsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OrderForms
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.OrderForms.Include(o => o.DocumentMenu).Include(o => o.Member);
            return View(await appDbContext.ToListAsync());
        }

        // GET: OrderForms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderForm = await _context.OrderForms
                .Include(o => o.DocumentMenu)
                .Include(o => o.Member)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orderForm == null)
            {
                return NotFound();
            }

            return View(orderForm);
        }

        // GET: OrderForms/Create
        public IActionResult Create()
        {
            ViewData["DocumentMenuId"] = new SelectList(_context.DocumentMenus, "MenuId", "MenuId");
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Email");
            return View();
        }

        // POST: OrderForms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,MemberId,DocumentMenuId,DepartureDate,ProcessingQuantity,ChineseSurname,ChineseName,EnglishSurname,EnglishName,Gender,BirthDate,ContactPersonName,ContactPersonEmail,ContactPersonPhoneNumber,PickupMethodOption,MailingCity,MailingDetailAddress,StoreDetailAddress,TaxIdOption,CompanyName,TaxIdNumber,OrderCreationTime")] OrderForm orderForm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderForm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DocumentMenuId"] = new SelectList(_context.DocumentMenus, "MenuId", "MenuId", orderForm.DocumentMenuId);
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Email", orderForm.MemberId);
            return View(orderForm);
        }

        // GET: OrderForms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderForm = await _context.OrderForms.FindAsync(id);
            if (orderForm == null)
            {
                return NotFound();
            }
            ViewData["DocumentMenuId"] = new SelectList(_context.DocumentMenus, "MenuId", "MenuId", orderForm.DocumentMenuId);
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Email", orderForm.MemberId);
            return View(orderForm);
        }

        // POST: OrderForms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,MemberId,DocumentMenuId,DepartureDate,ProcessingQuantity,ChineseSurname,ChineseName,EnglishSurname,EnglishName,Gender,BirthDate,ContactPersonName,ContactPersonEmail,ContactPersonPhoneNumber,PickupMethodOption,MailingCity,MailingDetailAddress,StoreDetailAddress,TaxIdOption,CompanyName,TaxIdNumber,OrderCreationTime")] OrderForm orderForm)
        {
            if (id != orderForm.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderForm);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderFormExists(orderForm.OrderId))
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
            ViewData["DocumentMenuId"] = new SelectList(_context.DocumentMenus, "MenuId", "MenuId", orderForm.DocumentMenuId);
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Email", orderForm.MemberId);
            return View(orderForm);
        }

        // GET: OrderForms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderForm = await _context.OrderForms
                .Include(o => o.DocumentMenu)
                .Include(o => o.Member)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orderForm == null)
            {
                return NotFound();
            }

            return View(orderForm);
        }

        // POST: OrderForms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderForm = await _context.OrderForms.FindAsync(id);
            if (orderForm != null)
            {
                _context.OrderForms.Remove(orderForm);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderFormExists(int id)
        {
            return _context.OrderForms.Any(e => e.OrderId == id);
        }
    }
}
