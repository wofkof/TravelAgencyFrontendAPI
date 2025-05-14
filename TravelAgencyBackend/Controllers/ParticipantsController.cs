using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgencyBackend.ViewModels;
using AutoMapper;
using TravelAgencyBackend.Services;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.Controllers
{
    public class ParticipantsController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly PermissionCheckService _perm;

        public ParticipantsController(AppDbContext context, IMapper mapper, PermissionCheckService perm)
            : base(perm)
        {
            _context = context;
            _mapper = mapper;
            _perm = perm;
        }

        public IActionResult Index(ParticipantIndexViewModel model, int? memberId)
        {
            var check = CheckPermissionOrForbid("查看參與人");
            if (check != null) return check;

            var query = _context.MemberFavoriteTravelers.Include(p => p.Member).AsQueryable();

            if (memberId.HasValue)
            {
                model.FilterMemberId = memberId;
            }

            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var keyword = model.SearchText.Trim();
                query = query.Where(p =>
                    p.Name.Contains(keyword) ||
                    p.Phone.Contains(keyword) ||
                    p.IdNumber.Contains(keyword));
            }

            if (model.FilterMemberId.HasValue)
            {
                query = query.Where(p => p.MemberId == model.FilterMemberId);
                var member = _context.Members.FirstOrDefault(m => m.MemberId == model.FilterMemberId);
                ViewBag.Title = $"【{member?.Name ?? "未知"}】的參與人";
            }
            else
            {
                ViewBag.Title = "全部參與人";
            }

            model.TotalCount = query.Count();

            model.Participants = _mapper.Map<List<ParticipantListItemViewModel>>(
                query.Skip((model.Page - 1) * model.PageSize).Take(model.PageSize).ToList()
            );

            model.Members = _context.Members.ToList();
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var check = CheckPermissionOrForbid("查看參與人");
            if (check != null) return check;

            if (id == null) return NotFound();

            var participant = await _context.MemberFavoriteTravelers.Include(p => p.Member).FirstOrDefaultAsync(p => p.FavoriteTravelerId == id);
            if (participant == null) return NotFound();

            var vm = _mapper.Map<ParticipantDetailViewModel>(participant);
            return View(vm);
        }

        private void SetFormOptions(object? selectedMemberId = null, string? selectedPlace = null)
        {
            ViewBag.Members = new SelectList(_context.Members, "MemberId", "Name", selectedMemberId);
            ViewBag.IssuedPlaces = new SelectList(new[] { "台北", "台中", "嘉義", "高雄", "花蓮" }, selectedPlace);
        }

        public IActionResult Create(int memberId)
        {
            var check = CheckPermissionOrForbid("管理參與人");
            if (check != null) return check;

            var member = _context.Members.Find(memberId);
            if (member == null) return NotFound($"找不到 ID 為 {memberId} 的參與人");

            var model = new ParticipantCreateViewModel { MemberId = memberId };
            ViewBag.MemberName = member.Name;
            SetFormOptions();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ParticipantCreateViewModel vm)
        {
            var check = CheckPermissionOrForbid("管理參與人");
            if (check != null) return check;

            if (_context.MemberFavoriteTravelers.Any(p => p.IdNumber == vm.IdNumber))
                ModelState.AddModelError("IdNumber", "身分證號已存在");

            if (_context.MemberFavoriteTravelers.Any(p => p.Phone == vm.Phone))
                ModelState.AddModelError("Phone", "手機已存在");

            if (!string.IsNullOrEmpty(vm.PassportNumber) && _context.MemberFavoriteTravelers.Any(p => p.DocumentNumber == vm.PassportNumber))
                ModelState.AddModelError("PassportNumber", "護照號碼已存在");

            if (!ModelState.IsValid)
            {
                SetFormOptions(vm.MemberId, vm.IssuedPlace);
                return View(vm);
            }

            var participant = _mapper.Map<MemberFavoriteTraveler>(vm);
            _context.MemberFavoriteTravelers.Add(participant);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index), new { memberId = vm.MemberId });
        }

        public IActionResult Edit(int id)
        {
            var check = CheckPermissionOrForbid("管理參與人");
            if (check != null) return check;

            var participant = _context.MemberFavoriteTravelers.Include(p => p.Member).FirstOrDefault(p => p.FavoriteTravelerId == id);
            if (participant == null) return NotFound($"找不到 ID 為 {id} 的參與人");

            var vm = _mapper.Map<ParticipantEditViewModel>(participant);
            ViewBag.MemberName = participant.Member.Name;
            SetFormOptions(participant.MemberId, participant.IssuedPlace);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ParticipantEditViewModel vm)
        {
            var check = CheckPermissionOrForbid("管理參與人");
            if (check != null) return check;

            if (id != vm.ParticipantId) return NotFound($"找不到 ID 為 {id} 的參與人");

            if (_context.MemberFavoriteTravelers.Any(p => p.IdNumber == vm.IdNumber && p.FavoriteTravelerId != vm.ParticipantId))
                ModelState.AddModelError("IdNumber", "身分證號已存在");

            if (_context.MemberFavoriteTravelers.Any(p => p.Phone == vm.Phone && p.FavoriteTravelerId != vm.ParticipantId))
                ModelState.AddModelError("Phone", "手機已存在");

            if (!string.IsNullOrEmpty(vm.PassportNumber) && _context.MemberFavoriteTravelers.Any
                (p => p.DocumentNumber == vm.PassportNumber && p.FavoriteTravelerId != vm.ParticipantId))
                ModelState.AddModelError("PassportNumber", "護照號碼已存在");

            if (!ModelState.IsValid)
            {
                SetFormOptions(vm.MemberId, vm.IssuedPlace);
                return View(vm);
            }

            var participant = _context.MemberFavoriteTravelers.Find(id);
            if (participant == null) return NotFound();

            _mapper.Map(vm, participant);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index), new { memberId = vm.MemberId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var check = CheckPermissionOrForbid("管理參與人");
            if (check != null) return check;

            var participant = _context.MemberFavoriteTravelers.Find(id);
            if (participant == null) return NotFound($"找不到 ID 為 {id} 的參與人");

            int memberId = participant.MemberId;
            _context.MemberFavoriteTravelers.Remove(participant);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index), new { memberId = memberId });
        }
    }
}
