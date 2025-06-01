using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.ViewModels;

namespace TravelAgencyBackend.Controllers
{
    public class CommentsController : Controller
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            var comments = await _context.Comments
                .Include(c => c.Member)
                .Include(c => c.OrderDetail)
                .ToListAsync();

            var groupTravels = await _context.GroupTravels
                .Include(gt => gt.OfficialTravelDetail)
                    .ThenInclude(od => od.OfficialTravel)
                .ToDictionaryAsync(gt => gt.GroupTravelId);

            var customTravels = await _context.CustomTravels
                .ToDictionaryAsync(ct => ct.CustomTravelId);

            var viewModels = comments.Select(c =>
            {
                string title = c.Category switch
                {
                    ProductCategory.GroupTravel =>
                        groupTravels.TryGetValue(c.OrderDetail.ItemId, out var gt)
                            ? gt.OfficialTravelDetail?.OfficialTravel?.Title ?? "團體行程"
                            : "團體行程",

                    ProductCategory.CustomTravel =>
                        customTravels.TryGetValue(c.OrderDetail.ItemId, out var ct)
                            ? ct.Note ?? "客製行程"
                            : "客製行程",

                    _ => "未知行程"
                };

                return new CommentViewModel
                {
                    CommentId = c.CommentId,
                    MemberId = c.MemberId,
                    OrderDetailId = c.OrderDetailId,
                    Category = c.Category,
                    Rating = c.Rating,
                    Content = c.Content,
                    MemberName = c.Member.Name,
                    ProductTitle = title,
                    CreatedAt = c.CreatedAt,
                    Status = c.Status
                };
            }).ToList();

            return View(viewModels);
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.Member)
                .Include(c => c.OrderDetail)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }

            string title = comment.Category switch
            {
                ProductCategory.GroupTravel => await _context.GroupTravels
                    .Include(gt => gt.OfficialTravelDetail)
                        .ThenInclude(od => od.OfficialTravel)
                    .Where(gt => gt.GroupTravelId == comment.OrderDetail.ItemId)
                    .Select(gt => gt.OfficialTravelDetail.OfficialTravel.Title)
                    .FirstOrDefaultAsync() ?? "團體行程",

                ProductCategory.CustomTravel => await _context.CustomTravels
                    .Where(ct => ct.CustomTravelId == comment.OrderDetail.ItemId)
                    .Select(ct => ct.Note)
                    .FirstOrDefaultAsync() ?? "客製行程",

                _ => "未知行程"
            };

            var vm = new CommentViewModel
            {
                CommentId = comment.CommentId,
                MemberId = comment.MemberId,
                OrderDetailId = comment.OrderDetailId,
                Category = comment.Category,
                Rating = comment.Rating,
                Content = comment.Content,
                MemberName = comment.Member.Name,
                ProductTitle = title,
                CreatedAt = comment.CreatedAt,
                Status = comment.Status
            };

            return View(vm);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Email");
            ViewData["OrderDetailId"] = new SelectList(_context.OrderDetails, "OrderDetailId", "OrderDetailId");

            return View(new CommentViewModel
            {
                Rating = 5,
                CreatedAt = DateTime.Now,
                Status = CommentStatus.Visible
            });
        }

        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommentViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var comment = new Comment
                {
                    MemberId = vm.MemberId,
                    OrderDetailId = vm.OrderDetailId,
                    Category = vm.Category,
                    Rating = vm.Rating,
                    Content = vm.Content,
                    CreatedAt = DateTime.Now,
                    Status = CommentStatus.Visible
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Email", vm.MemberId);
            ViewData["OrderDetailId"] = new SelectList(_context.OrderDetails, "OrderDetailId", "OrderDetailId", vm.OrderDetailId);
            return View(vm);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var vm = new CommentViewModel
            {
                CommentId = comment.CommentId,
                MemberId = comment.MemberId,
                OrderDetailId = comment.OrderDetailId,
                Category = comment.Category,
                Rating = comment.Rating,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                Status = comment.Status
            };

            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Email", comment.MemberId);
            ViewData["OrderDetailId"] = new SelectList(_context.OrderDetails, "OrderDetailId", "OrderDetailId", comment.OrderDetailId);
            return View(vm);
        }

        // POST: Comments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CommentViewModel vm)
        {
            if (id != vm.CommentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var comment = await _context.Comments.FindAsync(id);
                    if (comment == null) return NotFound();

                    comment.MemberId = vm.MemberId;
                    comment.OrderDetailId = vm.OrderDetailId;
                    comment.Category = vm.Category;
                    comment.Rating = vm.Rating;
                    comment.Content = vm.Content;
                    comment.Status = vm.Status;
                    comment.CreatedAt = vm.CreatedAt;

                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(vm.CommentId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Email", vm.MemberId);
            ViewData["OrderDetailId"] = new SelectList(_context.OrderDetails, "OrderDetailId", "OrderDetailId", vm.OrderDetailId);
            return View(vm);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var comment = await _context.Comments
                .Include(c => c.Member)
                .Include(c => c.OrderDetail)
                .FirstOrDefaultAsync(m => m.CommentId == id);

            if (comment == null) return NotFound();

            string title = comment.Category switch
            {
                ProductCategory.GroupTravel => await _context.GroupTravels
                    .Include(gt => gt.OfficialTravelDetail)
                        .ThenInclude(od => od.OfficialTravel)
                    .Where(gt => gt.GroupTravelId == comment.OrderDetail.ItemId)
                    .Select(gt => gt.OfficialTravelDetail.OfficialTravel.Title)
                    .FirstOrDefaultAsync() ?? "團體行程",

                ProductCategory.CustomTravel => await _context.CustomTravels
                    .Where(ct => ct.CustomTravelId == comment.OrderDetail.ItemId)
                    .Select(ct => ct.Note)
                    .FirstOrDefaultAsync() ?? "客製行程",

                _ => "未知行程"
            };

            var vm = new CommentViewModel
            {
                CommentId = comment.CommentId,
                MemberId = comment.MemberId,
                OrderDetailId = comment.OrderDetailId,
                Category = comment.Category,
                Rating = comment.Rating,
                Content = comment.Content,
                MemberName = comment.Member.Name,
                ProductTitle = title,
                CreatedAt = comment.CreatedAt,
                Status = comment.Status
            };

            return View(vm);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}
