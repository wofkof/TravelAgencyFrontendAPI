using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgencyFrontendAPI.DTOs.OfficialDTOs;
using TravelAgencyFrontendAPI.DTOs.OfficialDTOs.Search;
using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.Controllers.OfficialTravelControllers
{
    public class OfficialSearchController : Controller
    {
        private readonly AppDbContext _context;
        public OfficialSearchController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult> Detail(int id)
        {
            try
            {
                var travel = await (
                                from t in _context.OfficialTravels
                                where t.OfficialTravelId == id && t.Status == TravelStatus.Active
                                from d in t.OfficialTravelDetails
                                where d.TravelNumber == 1
                                from g in d.GroupTravels
                                where g.DepartureDate >= new DateTime(2025, 4, 1)
                                orderby g.DepartureDate // 建議加上排序才會是最近的
                                select new DetailDTO
                                {
                                    ProjectId = t.OfficialTravelId,
                                    Title = t.Title,
                                    Description = t.Description,
                                    Cover = t.CoverPath,
                                    Number = d.TravelNumber
                                }
                                  ).ToListAsync();

                // 第一次載入畫面取最近出團日GroupTravel資料顯示

                if (travel == null)
                {
                    return NotFound(new { message = "找不到對應專案" });
                }



                return Ok(travel);
            }
            catch(Exception ex) 
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
                                d.AdultPrice,
                                g.DepartureDate
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
                    .GroupBy(x => new { x.OfficialTravelId, x.Title, x.Description,x.CoverPath,x.AdultPrice })
                    .Select(g => new SearchOutput
                    {
                        ProjectId = g.Key.OfficialTravelId,
                        Title = g.Key.Title,
                        Description = g.Key.Description,
                        Cover = g.Key.CoverPath,
                        Price = g.Key.AdultPrice,
                    })
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SearchBox API Error: " + ex.ToString());
                return StatusCode(500, new { message = ex.Message });
            }
        }


    }
}
