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
                        GroupId = x.GroupTravelId,
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

        [HttpGet("getMainInfo/{projectId}/{detailId}/{groupId}")]
        public async Task<ActionResult> GetMainInfo(int projectId,int detailId, int groupId)
        {
            try 
            { 
                var travel = await(
                    from t in _context.OfficialTravels
                    where t.OfficialTravelId == projectId && t.Status == TravelStatus.Active
                    from d in t.OfficialTravelDetails
                    where d.OfficialTravelDetailId == detailId
                    from g in d.GroupTravels
                    where g.GroupTravelId == groupId
                    select new GetMainInfo
                    {
                        ProjectId = t.OfficialTravelId,
                        Title = t.Title,
                        Description = t.Description,
                        Cover = t.CoverPath,
                        Country = t.Region.Country,
                        Region = t.Region.Name,
                        Price = d.AdultPrice,
                        Departure = g.DepartureDate,
                        Return = g.ReturnDate
                    }
                ).FirstOrDefaultAsync();
                return Ok(travel);
            }
            catch (Exception ex) 
            { 
                Console.WriteLine("GetMainInfo API Error: " + ex.ToString());
                return StatusCode(500, new { message = ex.Message }); 
            }
        }

        [HttpGet("getGrouplist/{projectId}")]
        public async Task<ActionResult> GetGrouplist(int projectId)
        {
            try
            {
                var groups = await(
                    from g in _context.GroupTravels
                    from d in _context.OfficialTravelDetails
                    from t in _context.OfficialTravels
                    where g.OfficialTravelDetailId == d.OfficialTravelDetailId && d.OfficialTravelId == projectId
                    select new GetGroups
                    {
                        GroupId = g.GroupTravelId,
                        Departure = g.DepartureDate,
                        Return = g.ReturnDate,
                        TotalSeats = g.TotalSeats,
                        AvailableSeats = g.TotalSeats - g.SoldSeats,
                        GroupStatus = g.GroupStatus
                    }
                    ).ToListAsync();

                var result = groups
                    .GroupBy(x => new { x.GroupId})
                    .Select(g => new GetGroups
                    {
                        GroupId = g.Key.GroupId,
                        Departure = g.FirstOrDefault().Departure,
                        Return = g.FirstOrDefault().Return,
                        TotalSeats = g.FirstOrDefault().TotalSeats,
                        AvailableSeats = g.Sum(x => x.AvailableSeats),
                        GroupStatus = g.FirstOrDefault().GroupStatus
                    }
                    ).ToList();


                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetGroup API Error: " + ex.ToString());
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("GetSchedule/{detailId}")]
        public async Task<ActionResult> GetSchedule(int detailId)
        {
            try
            {
                var schedule = await (
                    from s in _context.OfficialTravelSchedules
                    select new GetSchedule
                    {
                        ScheduleId = s.OfficialTravelScheduleId,
                        Day = s.Day,
                        Description = s.Description,
                        Breakfast = s.Breakfast,
                        Lunch = s.Lunch,
                        Dinner = s.Dinner,
                        Hotel = s.Hotel,
                        Attraction1 = s.Attraction1,
                        Attraction2 = s.Attraction2,
                        Attraction3 = s.Attraction3,
                        Attraction4 = s.Attraction4,
                        Attraction5 = s.Attraction5
                    }
                    ).ToListAsync();
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetSchedule API Error: " + ex.ToString());
                return StatusCode(500, new { message = ex.Message });
            }
        }

        
    }
}
