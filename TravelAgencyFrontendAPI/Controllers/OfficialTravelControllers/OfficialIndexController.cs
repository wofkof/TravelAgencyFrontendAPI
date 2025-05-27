using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.OfficialDTOs;

namespace TravelAgencyFrontendAPI.Controllers.OfficialTravelControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficialIndexController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OfficialIndexController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("getcard/{category}")]
        public async Task<ActionResult> GetCardInfo(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return BadRequest(new { message = "發生錯誤，請稍後再試" });
            }
            if (!Enum.TryParse(category, out TravelCategory parsedCategory))
                return BadRequest("Invalid category");
            try 
            {
                var rawData =
                from t in _context.OfficialTravels
                where t.Category == parsedCategory && t.Status == TravelStatus.Active
                from d in t.OfficialTravelDetails
                where d.OfficialTravelId == t.OfficialTravelId
                from g in d.GroupTravels
                where g.OfficialTravelDetailId == d.OfficialTravelDetailId
                select new
                {
                    t.OfficialTravelId,
                    t.Title,
                    t.Description,
                    t.CoverPath,
                    t.Region.Country,
                    t.Region.Name,
                    t.Category,
                    d.OfficialTravelDetailId,
                    d.AdultPrice,
                    g.GroupTravelId,
                    g.DepartureDate,
                    g.ReturnDate
                };

                var result = await rawData
                    .GroupBy(x => new {
                        x.OfficialTravelId

                    })
                    .Select(g => g
                        .OrderBy(x => x.DepartureDate)
                        .Select(x => new SearchOutput
                        {
                            ProjectId = x.OfficialTravelId,
                            Title = x.Title,
                            Description = x.Description,
                            Category = x.Category,
                            Cover = x.CoverPath,
                            Price = x.AdultPrice,
                            DetailId = x.OfficialTravelDetailId,
                            GroupId = x.GroupTravelId,
                            DepartureDate = x.DepartureDate,
                            ReturnDate = x.ReturnDate,
                            Days = x.DepartureDate != null && x.ReturnDate != null ? (x.ReturnDate.Value - x.DepartureDate.Value).Days : 0,
                            Country = x.Country,
                            Region = x.Name,
                        }).FirstOrDefault()
                    ).ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchBox API Error: " + ex.ToString());
                return StatusCode(500, new { message = ex.Message });
            }
            
        }

        [HttpGet("getlistInfo/{category}/{keyword}")]
        public async Task<ActionResult> GetListInfo(string keyword, string category)
        {
            if (string.IsNullOrWhiteSpace(keyword) || string.IsNullOrWhiteSpace(category))
                return BadRequest(new { message = "發生錯誤，請稍後再試" });

            if (!Enum.TryParse(category, out TravelCategory parsedCategory))
                return BadRequest("Invalid category");

            try
            {
                // 從資料庫撈取所有相關資料
                var rawData = await _context.OfficialTravels
                    .Where(x => x.Category == parsedCategory)
                    .Include(x => x.Region)
                    .Include(x => x.OfficialTravelDetails)
                        .ThenInclude(d => d.GroupTravels)
                    .Include(x => x.OfficialTravelDetails)
                        .ThenInclude(d => d.officialTravelSchedules)
                    .ToListAsync();

                // 展平成可用資料集合
                var flatData = rawData.SelectMany(t => t.OfficialTravelDetails.SelectMany(d =>
                    d.GroupTravels.Select(g => new
                    {
                        t.OfficialTravelId,
                        t.Title,
                        t.Description,
                        t.CoverPath,
                        t.Category,
                        t.Days,
                        Country = t.Region.Country,
                        Region = t.Region.Name,
                        d.OfficialTravelDetailId,
                        d.AdultPrice,
                        g.GroupTravelId,
                        g.DepartureDate,
                        g.ReturnDate,
                        Schedules = d.officialTravelSchedules
                    })
                )).ToList();

                // 篩選關鍵字（如果不是 "全部"）
                if (keyword != "全部")
                {
                    flatData = flatData.Where(x =>
                        (x.Title?.Contains(keyword) ?? false) ||
                        (x.Description?.Contains(keyword) ?? false) ||
                        (x.Country?.Contains(keyword) ?? false) ||
                        (x.Region?.Contains(keyword) ?? false) ||
                        (x.Schedules?.Any(s =>
                            (s.Description?.Contains(keyword) ?? false) ||
                            (s.Breakfast?.Contains(keyword) ?? false) ||
                            (s.Lunch?.Contains(keyword) ?? false) ||
                            (s.Dinner?.Contains(keyword) ?? false) ||
                            (s.Hotel?.Contains(keyword) ?? false)) ?? false)
                    ).ToList();
                }

                // 根據 OfficialTravelId 分組，每組只取最早出發日期的那筆資料
                var result = flatData
                    .GroupBy(x => x.OfficialTravelId)
                    .Select(g => g
                        .OrderBy(x => x.DepartureDate)
                        .FirstOrDefault()
                    )
                    .Where(x => x != null)
                    .Select(x => new SearchOutput
                    {
                        ProjectId = x.OfficialTravelId,
                        Title = x.Title,
                        Description = x.Description,
                        Category = x.Category,
                        Cover = x.CoverPath,
                        Price = x.AdultPrice,
                        DetailId = x.OfficialTravelDetailId,
                        GroupId = x.GroupTravelId,
                        DepartureDate = x.DepartureDate,
                        ReturnDate = x.ReturnDate,
                        Days = x.DepartureDate.HasValue && x.ReturnDate.HasValue
                            ? (x.ReturnDate.Value - x.DepartureDate.Value).Days
                            : 0,
                        Country = x.Country,
                        Region = x.Region
                    })
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchBox API Error: " + ex);
                return StatusCode(500, new { message = ex.Message });
            }
        }





    }
}
