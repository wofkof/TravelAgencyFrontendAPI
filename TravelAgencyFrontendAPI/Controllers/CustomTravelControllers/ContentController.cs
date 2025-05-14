using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs.CustomTravelDTOs;
using TravelAgencyFrontendAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelAgencyFrontendAPI.Controllers.CustomTravelControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ContentController(AppDbContext context)
        {
            _context = context;
        }

        //POST: api/Content/Create
        [HttpPost("Create")]
        public async Task<IActionResult> PostContent([FromBody] CustomTravelInputDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var memberExists = await _context.Members.AnyAsync(m => m.MemberId == dto.MemberId);
            if (!memberExists)
            {
                return Unauthorized("會員驗證失敗");
            }

            var travel = new CustomTravel
            {
                MemberId = dto.MemberId,
                ReviewEmployeeId = _context.Employees.First().EmployeeId,
                CreatedAt = DateTime.Now,
                DepartureDate = dto.DepartureDate,
                EndDate = dto.EndDate,
                Days = dto.Days,
                People = dto.People,
                TotalAmount = dto.TotalAmount,
                Status = CustomTravelStatus.Pending,
                Note = dto.Note
            };

            _context.CustomTravels.Add(travel);
            await _context.SaveChangesAsync(); // 先存主表取ID

            foreach (var c in dto.Contents)
            {
                _context.CustomTravelContents.Add(new CustomTravelContent
                {
                    CustomTravelId = travel.CustomTravelId,
                    Day = c.Day,
                    Time = c.Time,
                    ItemId = c.ItemId,
                    Category = c.Category,
                    AccommodationName = c.AccommodationName
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { travelId = travel.CustomTravelId });
        }

        //GET: api/Content
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomTravelContentDto>>> GetCustomerTravelContent([FromQuery] int id)
        {
            var contents = await _context.CustomTravelContents
                .Where(c => c.CustomTravelId == id).ToListAsync();

            var CustomTravelContentDto = contents.Select(c => new CustomTravelContentDto 
            {
                CustomTravelId = c.CustomTravelId,
                ItemId = c.ItemId,
                Category = c.Category,
                Day = c.Day,
                Time = c.Time,
                AccommodationName= c.AccommodationName,
                ItemName = c.Category switch
                {
                    TravelItemCategory.Attraction => _context.Attractions.FirstOrDefault(a => a.AttractionId == c.ItemId)?.AttractionName ?? "未知",
                    TravelItemCategory.Restaurant => _context.Restaurants.FirstOrDefault(r => r.RestaurantId == c.ItemId)?.RestaurantName ?? "未知",
                    TravelItemCategory.Accommodation => _context.Accommodations.FirstOrDefault(a => a.AccommodationId == c.ItemId)?.AccommodationName ?? "未知",
                    TravelItemCategory.Transport => _context.Transports.FirstOrDefault(t => t.TransportId == c.ItemId)?.TransportMethod ?? "未知",
                    _ => "未知項目"
                }
            }).OrderBy(c => c.Time).ToList();
            return Ok(CustomTravelContentDto);
        }

    }
}
