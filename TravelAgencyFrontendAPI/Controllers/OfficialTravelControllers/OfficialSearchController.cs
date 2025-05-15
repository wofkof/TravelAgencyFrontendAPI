using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs.OfficialDTOs;
using TravelAgencyFrontendAPI.DTOs.OfficialDTOs.Search;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Controllers.OfficialTravelControllers
{
    public class OfficialSearchController : Controller
    {
        private readonly AppDbContext _context;
        public OfficialSearchController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("search")]
        public async Task<ActionResult> Search([FromBody] SearchBoxInputDTO dto)
        {
            if (dto.Destination == "") 
            {
                return BadRequest(new { message = "請輸入關鍵字" });
            }
            try
            {
                var result = await _context.OfficialTravels
                    .Include(t => t.OfficialTravelDetails)
                    .Include(t => t.Region)
                    .Where(t =>
                        (t.Title.Contains(dto.Destination) ||
                         t.Description.Contains(dto.Destination) ||
                         t.Region.Country.Contains(dto.Destination) ||
                         t.Region.Name.Contains(dto.Destination)) &&
                        t.Status == TravelStatus.Active
                    )
                    .Select(t => new SearchBoxResultDTO
                    {
                        Id = t.OfficialTravelId,
                        Title = t.Title,
                        Description = t.Description
                            
                    })
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                // 確保你看到錯誤
                Console.WriteLine("Search API Error: " + ex.ToString());
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult> Detail(int id)
        {
            try
            {
                var travel = await _context.OfficialTravels
                     .Where(o =>o.OfficialTravelId == id)
                     .Select(o =>new DetailDTO
                     {
                         ProjectId = o.OfficialTravelId,
                         Title = o.Title,
                         Description = o.Description
                     }).FirstOrDefaultAsync();

                if(travel == null)
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

    }
}
