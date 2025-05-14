using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgencyBackend.Services;
using TravelAgencyBackend.ViewModels;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace TravelAgencyBackend.Controllers
{
    public class CustomTravelProductsController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly PermissionCheckService _perm;
        public CustomTravelProductsController(AppDbContext context, PermissionCheckService perm)
            : base(perm)
        {
            _context = context;
            _perm = perm;
        }

        public IActionResult List(KeywordViewModel p)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            IEnumerable<City> City = null;
            if (string.IsNullOrEmpty(p.txtKeywordCity))
            {
                City = _context.Cities.OrderBy(d => d.CityId).ToList();
            }
            else
            City = _context.Cities.Where(d => d.CityName.Contains(p.txtKeywordCity)).OrderBy(d => d.CityId).ToList();

            IEnumerable<District> District = null;
            if (string.IsNullOrEmpty(p.txtKeywordDistrict))
            {
                District = _context.Districts.OrderBy(d => d.DistrictId).ToList();
            }
            else
                District = _context.Districts.Where(d => d.DistrictName.Contains(p.txtKeywordDistrict)).OrderBy(d => d.DistrictId).ToList();

            IEnumerable<Attraction> Attraction = null;
            if (string.IsNullOrEmpty(p.txtKeywordAttraction))
            {
                Attraction = _context.Attractions.OrderBy(d => d.DistrictId).ToList();
            }
            else
                Attraction = _context.Attractions.Where(d => d.ScenicSpotName.Contains(p.txtKeywordAttraction)).OrderBy(d => d.DistrictId).ToList();

            IEnumerable<Restaurant> Restaurant = null;
            if (string.IsNullOrEmpty(p.txtKeywordRestaurant))
            {
                Restaurant = _context.Restaurants.OrderBy(d => d.DistrictId).ToList();
            }
            else
                Restaurant = _context.Restaurants.Where(d => d.RestaurantName.Contains(p.txtKeywordRestaurant)).OrderBy(d => d.DistrictId).ToList();

            IEnumerable<Hotel> Hotel = null;
            if (string.IsNullOrEmpty(p.txtKeywordHotel))
            {
                Hotel = _context.Hotels.OrderBy(d => d.DistrictId).ToList();
            }
            else
                Hotel = _context.Hotels.Where(d => d.HotelName.Contains(p.txtKeywordHotel)).OrderBy(d => d.DistrictId).ToList();

            IEnumerable<Transportation> Transportation = null;
            if (string.IsNullOrEmpty(p.txtKeywordTransportation))
            {
                Transportation = _context.Transportations.OrderBy(d => d.TransportId).ToList();
            }
            else
                Transportation = _context.Transportations.Where(d => d.TransportMethod.Contains(p.txtKeywordTransportation)).OrderBy(d => d.TransportId).ToList();


            var datas = new CustomTravelProductsViewModel
            {
                City = City,
                District = District,
                Attraction = Attraction,
                Restaurant = Restaurant,
                Hotel = Hotel,
                Transportation = Transportation
            };
            return View(datas);
        }

        //City
        public IActionResult CreateCity()
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            return View();
        }
        [HttpPost]
        public IActionResult CreateCity(City p)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            //AppDbContext db = new AppDbContext();
            //db.Cities.Add(p.city);
            //db.SaveChanges();
            _context.Cities.Add(p);
            _context.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult DeleteCity(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id != null)
            {
                
                City d = _context.Cities.FirstOrDefault(p => p.CityId == id);
                if (d != null)
                {
                    _context.Cities.Remove(d);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("List");
        }
        public IActionResult EditCity(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id == null)
                return RedirectToAction("List");            
            City d = _context.Cities.FirstOrDefault(p => p.CityId == id);
            if (d == null)
                return RedirectToAction("List");
            return View(d);
        }
        [HttpPost]
        public IActionResult EditCity(City uiCity)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            City dbCity = _context.Cities.FirstOrDefault(p => p.CityId == uiCity.CityId);
            if (dbCity == null)
                return RedirectToAction("List");
            dbCity.CityName = uiCity.CityName;
            _context.SaveChanges();
            return RedirectToAction("List");
        }

        //District
        public IActionResult CreateDistrict()
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            ViewBag.Cities = new SelectList(_context.Cities, "CityId", "CityName");
            return View();
        }
        [HttpPost]
        public IActionResult CreateDistrict(District p)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (!_context.Cities.Any(c => c.CityId == p.CityId))
            {
                ModelState.AddModelError("CityId", "請選擇有效的城市");
            }

            _context.Districts.Add(p);
            _context.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult DeleteDistrict(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id != null)
            {                
                District d = _context.Districts.FirstOrDefault(p => p.DistrictId == id);
                if (d != null)
                {
                    _context.Districts.Remove(d);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("List");
        }
        public IActionResult EditDistrict(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id == null)
                return RedirectToAction("List");            
            District d = _context.Districts.FirstOrDefault(p => p.DistrictId == id);
            if (d == null)
                return RedirectToAction("List");
            ViewBag.Cities = new SelectList(_context.Cities, "CityId", "CityName");

            return View(d);
        }
        [HttpPost]
        public IActionResult EditDistrict(District uiDistrict)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            District dbDistrict = _context.Districts.FirstOrDefault(p => p.DistrictId == uiDistrict.DistrictId);
            if (dbDistrict == null)
                return RedirectToAction("List");
            dbDistrict.CityId = uiDistrict.CityId;
            dbDistrict.DistrictName = uiDistrict.DistrictName;

            if (!_context.Cities.Any(c => c.CityId == uiDistrict.CityId))
            {
                ModelState.AddModelError("CityId", "請選擇有效的城市");
            }
            _context.SaveChanges();
            return RedirectToAction("List");
        }

        //Attraction
        public IActionResult CreateAttraction()
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            ViewBag.Districts = new SelectList(_context.Districts, "DistrictId", "DistrictName");
            ViewBag.TravelSuppliers = new SelectList(_context.TravelSuppliers, "TravelSupplierId", "SupplierName");
            return View();
        }
        [HttpPost]
        public IActionResult CreateAttraction(Attraction p)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (!_context.Districts.Any(c => c.DistrictId == p.DistrictId))
            {
                ModelState.AddModelError("DistrictId", "請選擇有效的區");
            }
            if (!_context.TravelSuppliers.Any(c => c.TravelSupplierId == p.TravelSupplierId))
            {
                ModelState.AddModelError("TravelSupplierId", "請選擇有效的供應商");
            }

            _context.Attractions.Add(p);
            _context.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult DeleteAttraction(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id != null)
            {                
                Attraction d = _context.Attractions.FirstOrDefault(p => p.AttractionId == id);
                if (d != null)
                {
                    _context.Attractions.Remove(d);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("List");
        }
        public IActionResult EditAttraction(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id == null)
                return RedirectToAction("List");            
            Attraction d = _context.Attractions.FirstOrDefault(p => p.AttractionId == id);
            if (d == null)
                return RedirectToAction("List");
            ViewBag.Districts = new SelectList(_context.Districts, "DistrictId", "DistrictName");
            ViewBag.TravelSuppliers = new SelectList(_context.TravelSuppliers, "TravelSupplierId", "SupplierName");
            return View(d);
        }
        [HttpPost]
        public IActionResult EditAttraction(Attraction uiAttraction)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            Attraction dbAttraction =_context.Attractions.FirstOrDefault(p => p.AttractionId == uiAttraction.AttractionId);
            if (dbAttraction == null)
                return RedirectToAction("List");
            dbAttraction.DistrictId = uiAttraction.DistrictId;
            dbAttraction.TravelSupplierId = uiAttraction.TravelSupplierId;
            dbAttraction.ScenicSpotName = uiAttraction.ScenicSpotName;
            if (!_context.Districts.Any(c => c.DistrictId == uiAttraction.DistrictId))
            {
                ModelState.AddModelError("DistrictId", "請選擇有效的區");
            }
            if (!_context.TravelSuppliers.Any(c => c.TravelSupplierId == uiAttraction.TravelSupplierId))
            {
                ModelState.AddModelError("TravelSupplierId", "請選擇有效的供應商");
            }
            _context.SaveChanges();
            return RedirectToAction("List");
        }

        //Restaurant
        public IActionResult CreateRestaurant()
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            ViewBag.Districts = new SelectList(_context.Districts, "DistrictId", "DistrictName");
            ViewBag.TravelSuppliers = new SelectList(_context.TravelSuppliers, "TravelSupplierId", "SupplierName");
            return View();
        }
        [HttpPost]
        public IActionResult CreateRestaurant(Restaurant p)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (!_context.Districts.Any(c => c.DistrictId == p.DistrictId))
            {
                ModelState.AddModelError("DistrictId", "請選擇有效的區");
            }
            if (!_context.TravelSuppliers.Any(c => c.TravelSupplierId == p.TravelSupplierId))
            {
                ModelState.AddModelError("TravelSupplierId", "請選擇有效的供應商");
            }
            _context.Restaurants.Add(p);
            _context.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult DeleteRestaurant(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id != null)
            {               
                Restaurant d = _context.Restaurants.FirstOrDefault(p => p.RestaurantId == id);
                if (d != null)
                {
                    _context.Restaurants.Remove(d);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("List");
        }
        public IActionResult EditRestaurant(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id == null)
                return RedirectToAction("List");            
            Restaurant d = _context.Restaurants.FirstOrDefault(p => p.RestaurantId == id);
            if (d == null)
                return RedirectToAction("List");
            ViewBag.Districts = new SelectList(_context.Districts, "DistrictId", "DistrictName");
            ViewBag.TravelSuppliers = new SelectList(_context.TravelSuppliers, "TravelSupplierId", "SupplierName");
            return View(d);
        }
        [HttpPost]
        public IActionResult EditRestaurant(Restaurant uiRestaurant)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            Restaurant dbRestaurant = _context.Restaurants.FirstOrDefault(p => p.RestaurantId == uiRestaurant.RestaurantId);
            if (dbRestaurant == null)
                return RedirectToAction("List");
            dbRestaurant.DistrictId = uiRestaurant.DistrictId;
            dbRestaurant.TravelSupplierId = uiRestaurant.TravelSupplierId;
            dbRestaurant.RestaurantName = uiRestaurant.RestaurantName;
            if (!_context.Districts.Any(c => c.DistrictId == uiRestaurant.DistrictId))
            {
                ModelState.AddModelError("DistrictId", "請選擇有效的區");
            }
            if (!_context.TravelSuppliers.Any(c => c.TravelSupplierId == uiRestaurant.TravelSupplierId))
            {
                ModelState.AddModelError("TravelSupplierId", "請選擇有效的供應商");
            }
            _context.SaveChanges();
            return RedirectToAction("List");
        }

        //Hotel
        public IActionResult CreateHotel()
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            ViewBag.Districts = new SelectList(_context.Districts, "DistrictId", "DistrictName");
            ViewBag.TravelSuppliers = new SelectList(_context.TravelSuppliers, "TravelSupplierId", "SupplierName");
            return View();
        }
        [HttpPost]
        public IActionResult CreateHotel(Hotel p)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (!_context.Districts.Any(c => c.DistrictId == p.DistrictId))
            {
                ModelState.AddModelError("DistrictId", "請選擇有效的區");
            }
            if (!_context.TravelSuppliers.Any(c => c.TravelSupplierId == p.TravelSupplierId))
            {
                ModelState.AddModelError("TravelSupplierId", "請選擇有效的供應商");
            }
            _context.Hotels.Add(p);
            _context.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult DeleteHotel(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id != null)
            {               
                Hotel d = _context.Hotels.FirstOrDefault(p => p.HotelId == id);
                if (d != null)
                {
                    _context.Hotels.Remove(d);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("List");
        }
        public IActionResult EditHotel(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id == null)
                return RedirectToAction("List");           
            Hotel d = _context.Hotels.FirstOrDefault(p => p.HotelId == id);
            if (d == null)
                return RedirectToAction("List");
            ViewBag.Districts = new SelectList(_context.Districts, "DistrictId", "DistrictName");
            ViewBag.TravelSuppliers = new SelectList(_context.TravelSuppliers, "TravelSupplierId", "SupplierName");
            return View(d);
        }
        [HttpPost]
        public IActionResult EditHotel(Hotel uiHotel)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            Hotel dbHotel = _context.Hotels.FirstOrDefault(p => p.HotelId == uiHotel.HotelId);
            if (dbHotel == null)
                return RedirectToAction("List");
            dbHotel.DistrictId = uiHotel.DistrictId;
            dbHotel.TravelSupplierId = uiHotel.TravelSupplierId;
            dbHotel.HotelName = uiHotel.HotelName;
            if (!_context.Districts.Any(c => c.DistrictId == uiHotel.DistrictId))
            {
                ModelState.AddModelError("DistrictId", "請選擇有效的區");
            }
            if (!_context.TravelSuppliers.Any(c => c.TravelSupplierId == uiHotel.TravelSupplierId))
            {
                ModelState.AddModelError("TravelSupplierId", "請選擇有效的供應商");
            }
            _context.SaveChanges();
            return RedirectToAction("List");
        }

        //Transportation
        public IActionResult CreateTransportation()
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            return View();
        }
        [HttpPost]
        public IActionResult CreateTransportation(Transportation p)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            _context.Transportations.Add(p);
            _context.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult DeleteTransportation(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id != null)
            {                
                Transportation d = _context.Transportations.FirstOrDefault(p => p.TransportId == id);
                if (d != null)
                {
                    _context.Transportations.Remove(d);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("List");
        }
        public IActionResult EditTransportation(int? id)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            if (id == null)
                return RedirectToAction("List");
            Transportation d = _context.Transportations.FirstOrDefault(p => p.TransportId == id);
            if (d == null)
                return RedirectToAction("List");
            ViewBag.Districts = new SelectList(_context.Districts, "DistrictId", "DistrictName");
            return View(d);
        }
        [HttpPost]
        public IActionResult EditTransportation(Transportation uiTransportation)
        {
            var check = CheckPermissionOrForbid("管理客製化行程");
            if (check != null) return check;

            Transportation dbTransportation = _context.Transportations.FirstOrDefault(p => p.TransportId == uiTransportation.TransportId);
            if (dbTransportation == null)
                return RedirectToAction("List");
            dbTransportation.TransportMethod = uiTransportation.TransportMethod;
            _context.SaveChanges();
            return RedirectToAction("List");
        }
    }
}
