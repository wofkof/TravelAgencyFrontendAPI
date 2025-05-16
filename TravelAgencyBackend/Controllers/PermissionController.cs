using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.Services;
using TravelAgencyBackend.ViewModels;

namespace TravelAgencyBackend.Controllers
{
    public class PermissionController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly PermissionCheckService _perm;

        public PermissionController(AppDbContext context, IMapper mapper, PermissionCheckService perm)
            : base(perm)
        {
            _context = context;
            _mapper = mapper;
            _perm = perm;
        }

        // GET: PermissionController
        public IActionResult Index()
        {
            var check = CheckPermissionOrForbid("管理權限");
            if (check != null) return check;

            var permissions = _context.Permissions.OrderBy(p => p.PermissionId).ToList();
            var viewModels = _mapper.Map<List<PermissionViewModel>>(permissions);

            return View(viewModels);
        }

        // GET: PermissionController/Details/5
        public IActionResult Details(int id)
        {
            var check = CheckPermissionOrForbid("管理權限");
            if (check != null) return check;

            return View();
        }

        // GET: PermissionController/Create
        public IActionResult Create() 
        {
            var check = CheckPermissionOrForbid("管理權限");
            if (check != null) return check;

            return View();
        }

        // POST: PermissionController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PermissionViewModel model)
        {
            var check = CheckPermissionOrForbid("管理權限");
            if (check != null) return check;

            if (!ModelState.IsValid) return View(model);

            var entity = _mapper.Map<Permission>(model);
            _context.Permissions.Add(entity);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: PermissionController/Edit/5
        public IActionResult Edit(int id)
        {
            var check = CheckPermissionOrForbid("管理權限");
            if (check != null) return check;

            var entity = _context.Permissions.Find(id);
            if (entity == null) return NotFound($"查無 ID 為 {id} 參數");

            var model = _mapper.Map<PermissionViewModel>(entity);

            return View(model);
        }

        // POST: PermissionController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PermissionViewModel model)
        {
            var check = CheckPermissionOrForbid("管理權限");
            if (check != null) return check;

            if (!ModelState.IsValid) return View(model);

            var entity = _context.Permissions.Find(model.PermissionId);
            if (entity == null) return NotFound($"查無 {model} 參數");

            _mapper.Map(model, entity);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: PermissionController/Delete/5
        public IActionResult Delete(int id)
        {
            var check = CheckPermissionOrForbid("管理權限");
            if (check != null) return check;

            var entity = _context.Permissions.Find(id);
            if (entity == null) return NotFound($"查無 ID 為 {id} 參數");

            _context.Permissions.Remove(entity);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}
