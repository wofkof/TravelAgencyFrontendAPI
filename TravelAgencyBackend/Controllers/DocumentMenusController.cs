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
    public class DocumentMenusController : Controller
    {
        private readonly AppDbContext _context;

        public DocumentMenusController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DocumentMenus
        public async Task<IActionResult> Index()
        {
            return View(await _context.DocumentMenus.ToListAsync());
        }

        // GET: DocumentMenus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var documentMenu = await _context.DocumentMenus
                .FirstOrDefaultAsync(m => m.MenuId == id);
            if (documentMenu == null)
            {
                return NotFound();
            }

            return View(documentMenu);
        }

        // GET: DocumentMenus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DocumentMenus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MenuId,RocPassportOption,ForeignVisaOption,ApplicationType,ProcessingItem,CaseType,ProcessingDays,DocumentValidityPeriod,StayDuration,Fee")] DocumentMenu documentMenu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(documentMenu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(documentMenu);
        }

        // GET: DocumentMenus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var documentMenu = await _context.DocumentMenus.FindAsync(id);
            if (documentMenu == null)
            {
                return NotFound();
            }
            return View(documentMenu);
        }

        // POST: DocumentMenus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MenuId,RocPassportOption,ForeignVisaOption,ApplicationType,ProcessingItem,CaseType,ProcessingDays,DocumentValidityPeriod,StayDuration,Fee")] DocumentMenu documentMenu)
        {
            if (id != documentMenu.MenuId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(documentMenu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentMenuExists(documentMenu.MenuId))
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
            return View(documentMenu);
        }

        // GET: DocumentMenus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var documentMenu = await _context.DocumentMenus
                .FirstOrDefaultAsync(m => m.MenuId == id);
            if (documentMenu == null)
            {
                return NotFound();
            }

            return View(documentMenu);
        }

        // POST: DocumentMenus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var documentMenu = await _context.DocumentMenus.FindAsync(id);
            if (documentMenu != null)
            {
                _context.DocumentMenus.Remove(documentMenu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocumentMenuExists(int id)
        {
            return _context.DocumentMenus.Any(e => e.MenuId == id);
        }
    }
}
