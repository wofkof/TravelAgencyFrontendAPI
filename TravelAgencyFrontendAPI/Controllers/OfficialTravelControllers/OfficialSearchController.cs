using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgencyFrontendAPI.DTOs.OfficialDTOs;
using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.Controllers.OfficialTravelControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficialSearchController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OfficialSearchController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("detail/{projectId}/{detailId}/{groupId}")]
        public async Task<ActionResult> Detail(int projectId, int detailId, int groupId)
        {
            try
            {
                var raw = await (
                    from t in _context.OfficialTravels
                    where t.OfficialTravelId == projectId && t.Status == TravelStatus.Active
                    from d in t.OfficialTravelDetails
                    where d.OfficialTravelDetailId == detailId
                    from s in d.officialTravelSchedules
                    where s.OfficialTravelDetailId == detailId
                    from g in d.GroupTravels
                    where g.GroupTravelId == groupId
                    orderby g.DepartureDate
                    select new DetailDTO
                    {
                        ProjectId = t.OfficialTravelId,
                        Title = t.Title,
                        Description = t.Description,
                        Cover = t.CoverPath,
                        DetailId = d.OfficialTravelDetailId,
                        Number = d.TravelNumber,
                        AdultPrice = d.AdultPrice,
                        GroupTravelId = g.GroupTravelId,
                        DepartureDate = g.DepartureDate,
                        ReturnDate = g.ReturnDate,
                        AvailableSeats = g.TotalSeats - g.SoldSeats,
                        TotalSeats = g.TotalSeats,
                        ScheduleId = s.OfficialTravelScheduleId,
                        ScheduleDescription = s.Description,
                        Day = s.Day,
                        Breakfast = s.Breakfast,
                        Lunch = s.Lunch,
                        Dinner = s.Dinner,
                        Hotel = s.Hotel,
                        Attraction1 = s.Attraction1,
                        Attraction2 = s.Attraction2,
                        Attraction3 = s.Attraction3,
                        Attraction4 = s.Attraction4,
                        Attraction5 = s.Attraction5,
                    }
                ).ToListAsync();

                var result = raw
                    .GroupBy(x => new { x.ProjectId, x.DetailId, x.GroupTravelId }) // 確保每一組唯一行程
                    .Select(g => new TravelDetailResultDto
                    {
                        ProjectId = g.Key.ProjectId,
                        DetailId = g.Key.DetailId,
                        Title = g.First().Title,
                        Description = g.First().Description,
                        Cover = g.First().Cover,
                        Number = g.First().Number,
                        AdultPrice = g.First().AdultPrice,
                        GroupTravelId = g.Key.GroupTravelId,
                        DepartureDate = g.First().DepartureDate,
                        ReturnDate = g.First().ReturnDate,
                        AvailableSeats = g.First().AvailableSeats,
                        TotalSeats = g.First().TotalSeats,
                        Schedules = g
                            .OrderBy(s => s.Day)
                            .Select(s => new TravelScheduleDto
                            {
                                ScheduleId = s.ScheduleId,
                                Day = s.Day,
                                ScheduleDescription = s.ScheduleDescription,
                                Breakfast = s.Breakfast,
                                Lunch = s.Lunch,
                                Dinner = s.Dinner,
                                Hotel = s.Hotel,
                                Attraction1 = s.Attraction1,
                                Attraction2 = s.Attraction2,
                                Attraction3 = s.Attraction3,
                                Attraction4 = s.Attraction4,
                                Attraction5 = s.Attraction5
                            }).ToList(),
                        GroupTravels = g
                            .Select(g => new GroupTravelDto
                            {
                                GroupTravelId = g.GroupTravelId,
                                DepartureDate = g.DepartureDate,
                                ReturnDate = g.ReturnDate,
                                AvailableSeats = g.TotalSeats - g.SoldSeats,
                                TotalSeats = g.TotalSeats,
                                Price = g.AdultPrice,
                                StatusText = g.GroupStatus
                            }).ToList()
                    })
                    .FirstOrDefault(); // 因為只有一筆 groupTravelId，直接取第一筆即可


                if (result == null)
                {
                    return NotFound(new { message = "找不到對應專案" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Search API Error: " + ex.ToString());
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult> SearchBox([FromQuery] SearchInput dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Destination))
            {
                return BadRequest(new { message = "請輸入關鍵字" });
            }

            try
            {
                // 先展開 GroupTravel 為主體
                var query = from t in _context.OfficialTravels
                            where t.Status == TravelStatus.Active &&
                                  (t.Title.Contains(dto.Destination) ||
                                   t.Description.Contains(dto.Destination) ||
                                   t.Region.Country.Contains(dto.Destination) ||
                                   t.Region.Name.Contains(dto.Destination))
                            from d in t.OfficialTravelDetails
                            from g in d.GroupTravels
                            where g.TotalSeats - g.SoldSeats >= dto.PeopleCount
                            select new
                            {
                                t.OfficialTravelId,
                                t.Title,
                                t.Description,
                                t.CoverPath,
                                t.Region.Country,
                                t.Region.Name,
                                d.OfficialTravelDetailId,
                                d.AdultPrice,
                                g.GroupTravelId,
                                g.DepartureDate,
                                g.GroupStatus
                            };

                // 加上動態條件
                if (dto.StartDate.HasValue)
                {
                    query = query.Where(x => x.DepartureDate >= dto.StartDate.Value);
                }

                if (dto.EndDate.HasValue)
                {
                    query = query.Where(x => x.DepartureDate <= dto.EndDate.Value);
                }

                // 最後 GroupBy 移除重複行程
                var result = await query
                .GroupBy(x => new {
                    x.OfficialTravelId,
                    x.Title,
                    x.Description,
                    x.CoverPath,
                    x.AdultPrice,
                    x.OfficialTravelDetailId
                })
                .Select(g => g
                    .OrderBy(x => x.DepartureDate)
                    .Select(x => new SearchOutput
                    {
                        ProjectId = x.OfficialTravelId,
                        Title = x.Title,
                        Description = x.Description,
                        Cover = x.CoverPath,
                        Price = x.AdultPrice,
                        DetailId = x.OfficialTravelDetailId,
                        GroupTravelId = x.GroupTravelId,
                        DepartureDate = x.DepartureDate,
                        Status = x.GroupStatus,
                        Country = x.Country,
                        Region = x.Name,
                    }).FirstOrDefault()
                )
                .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchBox API Error: " + ex.ToString());
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private AttractionDto? MapAttraction(int? id, List<OfficialAttraction> allAttractions)
        {
            if (id == null) return null;

            var a = allAttractions.FirstOrDefault(x => x.AttractionId == id.Value);
            if (a == null) return null;

            return new AttractionDto
            {
                AttractionId = a.AttractionId,
                Name = a.Name,
                Description = a.Description,
                Longitude = a.Longitude,
                Latitude = a.Latitude
            };
        }

        private HotelDto? MapHotel(int id, List<OfficialAccommodation> allHotels)
        {
            var h = allHotels.FirstOrDefault(x => x.AccommodationId == id);
            if (h == null) return null;

            return new HotelDto
            {
                AccommodationId = h.AccommodationId,
                Name = h.Name,
                Description = h.Description,
                Longitude = h.Longitude,
                Latitude = h.Latitude
            };
        }


    }
}
