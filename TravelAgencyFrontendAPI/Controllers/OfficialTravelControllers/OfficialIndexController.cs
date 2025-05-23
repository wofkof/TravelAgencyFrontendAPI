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

        [HttpGet("{category}")]
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

        //[HttpGet("{keyword}")]
        //public async Task<ActionResult> GetListInfo(string keyword)
        //{
        //    if (string.IsNullOrWhiteSpace(keyword))
        //    {
        //        return BadRequest(new { message = "發生錯誤，請稍後再試" });
        //    }
        //    //從標籤篩選(預設全部)
        //    //分組顯示
        //    try
        //    {
        //        var rawData =
        //        from t in _context.OfficialTravels
        //        where t.Title.Contains(keyword) || t.Description.Contains(keyword) || t.Region.Country.Contains(keyword) || t.Region.Name.Contains(keyword)
        //        from d in t.OfficialTravelDetails
        //        where d.OfficialTravelId == t.OfficialTravelId
        //        from g in d.GroupTravels
        //        where g.OfficialTravelDetailId == d.OfficialTravelDetailId
        //        from s in d.officialTravelSchedules
        //        where s.OfficialTravelDetailId == d.OfficialTravelDetailId && (s.Description.Contains(keyword) ||s.Breakfast.Contains(keyword) || s.Lunch.Contains(keyword) || s.Dinner.Contains(keyword) || s.Hotel.Contains(keyword))
        //        select new
        //        {
        //            t.OfficialTravelId,
        //            t.Title,
        //            t.Description,
        //            t.CoverPath,
        //            t.Region.Country,
        //            t.Region.Name,
        //            t.Category,
        //            d.OfficialTravelDetailId,
        //            d.AdultPrice,
        //            g.GroupTravelId,
        //            g.DepartureDate,
        //            g.ReturnDate
        //        };

        //        var result = await rawData
        //            .GroupBy(x => new {
        //                x.OfficialTravelId

        //            })
        //            .Select(g => g
        //                .OrderBy(x => x.DepartureDate)
        //                .Select(x => new SearchOutput
        //                {
        //                    ProjectId = x.OfficialTravelId,
        //                    Title = x.Title,
        //                    Description = x.Description,
        //                    Category = x.Category,
        //                    Cover = x.CoverPath,
        //                    Price = x.AdultPrice,
        //                    DetailId = x.OfficialTravelDetailId,
        //                    GroupId = x.GroupTravelId,
        //                    DepartureDate = x.DepartureDate,
        //                    ReturnDate = x.ReturnDate,
        //                    Days = x.DepartureDate != null && x.ReturnDate != null ? (x.ReturnDate.Value - x.DepartureDate.Value).Days : 0,
        //                    Country = x.Country,
        //                    Region = x.Name,
        //                }).FirstOrDefault()
        //            ).ToListAsync();

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("SearchBox API Error: " + ex.ToString());
        //        return StatusCode(500, new { message = ex.Message });
        //    }

        //}
    }
}
