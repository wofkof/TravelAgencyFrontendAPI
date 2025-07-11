﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.Services;
using TravelAgencyBackend.ViewModels;

namespace TravelAgencyBackend.Controllers
{
    public class CustomTravelPendingController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly PermissionCheckService _perm;
        public CustomTravelPendingController(AppDbContext context, PermissionCheckService perm)
            : base(perm)
        {
            _context = context;
            _perm = perm;
        }

        public IActionResult List(KeywordViewModel p)
        {
            var check = CheckPermissionOrForbid("查看客製化行程");
            if (check != null) return check;

            //IEnumerable<CustomTravel> CustomTravel = null;
            //if (string.IsNullOrEmpty(p.txtKeyword))
            //{
            //    CustomTravel = from d in _context.CustomTravels
            //                   select d;
            //}
            //else
            //    CustomTravel = _context.CustomTravels.Where(d =>
            //    d.MemberId.ToString().Contains(p.txtKeyword)
            //    || d.ReviewEmployeeId.ToString().Contains(p.txtKeyword)
            //    );
            //var datas = new CustomTravelPendingViewModel
            //{
            //    CustomTravel = CustomTravel
            //};
            IQueryable<CustomTravel> query = _context.CustomTravels
        .Include(x => x.Member)
        .Include(x => x.ReviewEmployee);

            if (!string.IsNullOrEmpty(p.txtKeyword))
            {
                query = query.Where(d =>
                    d.MemberId.ToString().Contains(p.txtKeyword) ||
                    d.ReviewEmployeeId.ToString().Contains(p.txtKeyword));
            }

            var datas = new CustomTravelPendingViewModel
            {
                CustomTravel = query.ToList()
            };
            return View(datas);
        }


        public IActionResult DeleteOrder(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id != null)
            {

                CustomTravel d = _context.CustomTravels.FirstOrDefault(p => p.CustomTravelId == id);
                if (d != null)
                {
                    _context.CustomTravels.Remove(d);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("List");
        }
        public IActionResult EditOrder(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id == null)
                return RedirectToAction("List");
            CustomTravel d = _context.CustomTravels.FirstOrDefault(p => p.CustomTravelId == id);
            if (d == null)
                return RedirectToAction("List");
            ViewBag.Members = new SelectList(_context.Members, "MemberId", "Name");
            ViewBag.ReviewEmployees = new SelectList(_context.Employees, "EmployeeId", "Name");
            return View(d);
        }
        [HttpPost]
        public IActionResult EditOrder(CustomTravel uiCustomTravel)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            CustomTravel dbCustomTravel = _context.CustomTravels.FirstOrDefault(p => p.CustomTravelId == uiCustomTravel.CustomTravelId);
            if (dbCustomTravel == null)
                return RedirectToAction("List");
            dbCustomTravel.MemberId = uiCustomTravel.MemberId;
            dbCustomTravel.ReviewEmployeeId = uiCustomTravel.ReviewEmployeeId;
            dbCustomTravel.CreatedAt = uiCustomTravel.CreatedAt;
            dbCustomTravel.UpdatedAt = DateTime.Now;
            dbCustomTravel.DepartureDate = uiCustomTravel.DepartureDate;
            dbCustomTravel.EndDate = uiCustomTravel.EndDate;
            dbCustomTravel.Days = uiCustomTravel.Days;
            dbCustomTravel.People = uiCustomTravel.People;
            dbCustomTravel.TotalAmount = uiCustomTravel.TotalAmount;
            dbCustomTravel.Status = uiCustomTravel.Status;
            dbCustomTravel.Note = uiCustomTravel.Note;

            if (!_context.Members.Any(c => c.MemberId == uiCustomTravel.MemberId))
            {
                ModelState.AddModelError("MemberId", "請選擇有效的會員");
            }
            if (!_context.Employees.Any(c => c.EmployeeId == uiCustomTravel.ReviewEmployeeId))
            {
                ModelState.AddModelError("EmployeeId", "請選擇有效的審核員工");
            }

            _context.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult ContentList(int? id)
        {
            var check = CheckPermissionOrForbid("查看客製化行程");
            if (check != null) return check;

            IEnumerable<CustomTravel> CustomTravel = null;
            if (id != null)
            {
                var customTravel = _context.CustomTravels
            .Where(d => d.CustomTravelId == id)
            .ToList();
                var content = _context.CustomTravelContents
                    .Where(c => c.CustomTravelId == id)
                    .OrderBy(c => c.Day)
                    .ThenBy(c => c.Time)
                    .ToList();
                var attractions = _context.Attractions.ToList();
                var restaurants = _context.Restaurants.ToList();
                var hotels = _context.Accommodations.ToList();
                var transportations = _context.Transports.ToList();

                var ViewModel = new CustomTravelPendingViewModel
                {
                    CustomTravel = customTravel,
                    Content = content,
                    Attraction = attractions,
                    Restaurant = restaurants,
                    Hotel = hotels,
                    Transportation = transportations
                };

                return View(ViewModel);
            }
            return RedirectToAction("List");
        }
        public IActionResult CreateContent()
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            var id = _context.CustomTravelContents.FirstOrDefault()?.CustomTravelId;

            var datas = new CustomTravelPendingViewModel
            {
                NewContent = new CustomTravelContent { CustomTravelId = id.Value },
                Content = _context.CustomTravelContents.ToList(),
                City = _context.Cities.ToList(),
                District = _context.Districts.ToList(),
                Attraction = _context.Attractions.ToList(),
                Restaurant = _context.Restaurants.ToList(),
                Hotel = _context.Accommodations.ToList(),
                Transportation = _context.Transports.ToList()
            };

            return View(datas);
        }
        [HttpPost]
        public IActionResult CreateContent(CustomTravelPendingViewModel p)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            _context.CustomTravelContents.Add(p.NewContent);
            _context.SaveChanges();
            return RedirectToAction("ContentList", new { id = p.NewContent.CustomTravelId });
        }
        public IActionResult DeleteContent(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id != null)
            {
                CustomTravelContent d = _context.CustomTravelContents.FirstOrDefault(p => p.ContentId == id);
                if (d != null)
                {
                    _context.CustomTravelContents.Remove(d);
                    _context.SaveChanges();

                    int? customTravelId = d.CustomTravelId;
                    return RedirectToAction("ContentList", new { id = customTravelId });
                }
            }
            return RedirectToAction("List");
        }
        public IActionResult EditContent(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id == null)
                return RedirectToAction("ContentList", new { id });
            CustomTravelContent d = _context.CustomTravelContents.FirstOrDefault(p => p.ContentId == id);
            if (d == null)
                return RedirectToAction("ContentList", new { id });

            var datas = new CustomTravelPendingViewModel
            {
                EditContent = d,
                City = _context.Cities.ToList(),
                District = _context.Districts.ToList(),
                Attraction = _context.Attractions.ToList(),
                Restaurant = _context.Restaurants.ToList(),
                Hotel = _context.Accommodations.ToList(),
                Transportation = _context.Transports.ToList()
            };
            return View(datas);
        }
        [HttpPost]
        public IActionResult EditContent(CustomTravelPendingViewModel t)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            CustomTravelContent dbContent = _context.CustomTravelContents.FirstOrDefault(p => p.ContentId == t.EditContent.ContentId);
            if (dbContent == null)
                return RedirectToAction("ContentList", new { id = t.EditContent.CustomTravelId });

            dbContent.CustomTravelId = t.EditContent.CustomTravelId;
            dbContent.ItemId = t.EditContent.ItemId;
            dbContent.Category = t.EditContent.Category;
            dbContent.Day = t.EditContent.Day;
            dbContent.Time = t.EditContent.Time;
            dbContent.AccommodationName = t.EditContent.AccommodationName;

            _context.SaveChanges();

            return RedirectToAction("ContentList", new { id = dbContent.CustomTravelId });
        }
    }
}