﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgencyBackend.ViewModels;
using TravelAgencyBackend.Helpers;
using AutoMapper;
using TravelAgencyBackend.ViewComponent;
using TravelAgencyBackend.Services;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.Controllers
{
    public class MemberController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly PermissionCheckService _perm;

        public MemberController(AppDbContext context, IMapper mapper, PermissionCheckService perm) 
            : base(perm) 
        {
            _context = context;
            _mapper = mapper;
            _perm = perm;
        }

        private void ValidateDuplicateContact(string email, string phone, int? excludeId = null)
        {
            if (_context.Members.Any(m => m.Email == email && (!excludeId.HasValue || m.MemberId != excludeId)))
                ModelState.AddModelError("Email", "此信箱已被註冊");

            if (_context.Members.Any(m => m.Phone == phone && (!excludeId.HasValue || m.MemberId != excludeId)))
                ModelState.AddModelError("Phone", "此手機號碼已被註冊");
        }

        // 修改密碼（GET）
        public IActionResult ChangePassword(int id)
        {
            var check = CheckPermissionOrForbid("修改會員密碼");
            if (check != null) return check;

            var member = _context.Members.Find(id);
            if (member == null) return NotFound($"找不到 ID 為 {id} 的會員");

            ViewBag.MemberId = id;
            ViewBag.Email = member.Email;
            return View();
        }

        // 修改密碼（POST）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(int id, string newPassword, string confirmPassword)
        {
            var check = CheckPermissionOrForbid("修改會員密碼");
            if (check != null) return check;

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "密碼與確認密碼不一致");
                ViewBag.MemberId = id;
                return View();
            }

            var member = _context.Members.Find(id);
            if (member == null) return NotFound($"找不到 ID 為 {id} 的會員");

            PasswordHasher.CreatePasswordHash(newPassword, out string hash, out string salt);

            member.PasswordHash = hash;
            member.PasswordSalt = salt;
            member.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id });
        }

        // Index（含搜尋與分頁）
        public IActionResult Index(MemberIndexViewModel model)
        {
            var check = CheckPermissionOrForbid("查看會員");
            if (check != null) return check;

            string keyword = model.SearchText?.Trim() ?? "";

            var query = _context.Members.AsNoTracking()
                .Where(m =>
                    (string.IsNullOrEmpty(keyword)
                     || m.Name.Contains(keyword)
                     || m.Phone.Contains(keyword)
                     || m.Email.Contains(keyword))
                    &&
                    (!model.FilterStatus.HasValue || m.Status == model.FilterStatus.Value)
                );

            model.TotalCount = query.Count();

            var result = query
                .OrderBy(m => m.MemberId)
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToList();

            model.Members = _mapper.Map<List<MemberListItemViewModel>>(result);
            return View(model);
        }

        // 詳細資料
        public IActionResult Details(int id)
        {
            var check = CheckPermissionOrForbid("查看會員");
            if (check != null) return check;

            var member = _context.Members.Find(id);
            if (member == null) return NotFound();

            var vm = _mapper.Map<MemberDetailViewModel>(member);
            return View(vm);
        }

        // 建立會員（GET）
        public IActionResult Create()
        {
            var check = CheckPermissionOrForbid("管理會員");
            if (check != null) return check;

            return View();
        }

        // 建立會員（POST）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MemberCreateViewModel vm)
        {
            var check = CheckPermissionOrForbid("管理會員");
            if (check != null) return check;

            ValidateDuplicateContact(vm.Email, vm.Phone);

            if (!ModelState.IsValid)
                return View(vm);

            PasswordHasher.CreatePasswordHash(vm.Password, out string hash, out string salt);

            var member = _mapper.Map<Member>(vm);
            member.PasswordHash = hash;
            member.PasswordSalt = salt;
            member.RegisterDate = DateTime.Now;
            member.UpdatedAt = DateTime.Now;
            member.Status = MemberStatus.Active;

            _context.Members.Add(member);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // 編輯會員（GET）
        public IActionResult Edit(int id)
        {
            var check = CheckPermissionOrForbid("管理會員");
            if (check != null) return check;

            var member = _context.Members.Find(id);
            if (member == null) return NotFound($"找不到 ID 為 {id} 的會員");

            var vm = _mapper.Map<MemberEditViewModel>(member);
            return View(vm);
        }
       
        // 編輯會員（POST）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, MemberEditViewModel vm)
        {
            var check = CheckPermissionOrForbid("管理會員");
            if (check != null) return check;

            if (id != vm.MemberId) return BadRequest("參數錯誤：id 不一致");

            ValidateDuplicateContact(vm.Email, vm.Phone, vm.MemberId);

            if (!ModelState.IsValid) return View(vm);

            var member = _context.Members.Find(id);
            if (member == null) return NotFound($"找不到 ID 為 {id} 的會員");

            member.Name = vm.Name;
            member.Email = vm.Email;
            member.Phone = vm.Phone;
            member.Gender = vm.Gender;
            member.Birthday = vm.Birthday;
            member.Nationality = vm.Nationality;
            member.PassportSurname = vm.PassportSurname;
            member.PassportGivenName = vm.PassportGivenName;
            member.IdNumber = vm.IdNumber;
            member.Address = vm.Address;
            member.Status = vm.Status;
            member.Note = vm.Note;
            member.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // 刪除會員
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var check = CheckPermissionOrForbid("管理會員");
            if (check != null) return check;

            var member = _context.Members.Find(id);
            if (member == null) return NotFound($"找不到 ID 為 {id} 的會員");

            _context.Members.Remove(member);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
