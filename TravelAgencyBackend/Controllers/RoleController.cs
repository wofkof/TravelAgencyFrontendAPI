using Microsoft.AspNetCore.Mvc;
using TravelAgency.Shared.Data;
using TravelAgencyBackend.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

public class RoleController : Controller
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public RoleController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public IActionResult Permissions(int id)
    {
        var role = _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefault(r => r.RoleId == id);

        if (role == null) return NotFound($"查無 ID 為 {id} 參數");

        var allPermissions = _context.Permissions.ToList();

        var vm = new RolePermissionViewModel
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Permissions = allPermissions.Select(p => new PermissionCheckboxItem
            {
                PermissionId = p.PermissionId,
                PermissionName = p.PermissionName,
                IsSelected = role.RolePermissions.Any(rp => rp.PermissionId == p.PermissionId)
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Permissions(RolePermissionViewModel model) 
    {
        var existing = _context.RolePermissions
            .Where(rp => rp.RoleId == model.RoleId)
            .ToList();

        _context.RolePermissions.RemoveRange(existing);

        var selectedIds = model.Permissions
            .Where(p => p.IsSelected)
            .Select(p => p.PermissionId)
            .ToList();

        foreach (var pid in selectedIds)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = model.RoleId,
                PermissionId = pid,
                CreatedAt = DateTime.Now,
            });
        }

        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    // GET: Role
    public IActionResult Index()
    {
        var roles = _context.Roles
            .OrderBy(r => r.RoleId)
            .ToList();

        var viewModels = _mapper.Map<List<RoleViewModel>>(roles);
        return View(viewModels);
    }

    // GET: Role/Create
    public IActionResult Create() => View();

    // POST: Role/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(RoleViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var entity = _mapper.Map<Role>(model);
        _context.Roles.Add(entity);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // GET: Role/Edit/5
    public IActionResult Edit(int id)
    {
        var role = _context.Roles.Find(id);
        if (role == null) return NotFound();

        var viewModel = _mapper.Map<RoleViewModel>(role);
        return View(viewModel);
    }

    // POST: Role/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(RoleViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var role = _context.Roles.Find(model.RoleId);
        if (role == null) return NotFound();

        _mapper.Map(model, role); // 更新實體
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // GET: Role/Delete/5
    public IActionResult Delete(int id)
    {
        var role = _context.Roles.Find(id);
        if (role == null) return NotFound();

        _context.Roles.Remove(role);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
}
