using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.DTOs.CustomTravelDTOs;
using TravelAgency.Shared.Models;
using TravelAgency.Shared.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelAgencyFrontendAPI.Controllers.CustomTravelControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ListController(AppDbContext context)
        {
            _context = context;
        }

        //GET: api/List
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomTravelDto>>> GetCustomerTravel([FromQuery] int memberId)
        {
            var memberExists = await _context.Members.AnyAsync(m => m.MemberId == memberId);
            if (!memberExists)
            {
                return Unauthorized("會員驗證失敗");
            }

            var CustomTravelDto = await _context.CustomTravels
                .Where(c => c.MemberId == memberId)
                .Select(c => new CustomTravelDto 
            {
                CustomTravelId = c.CustomTravelId,
                MemberId = c.MemberId,
                ReviewEmployeeId = c.ReviewEmployeeId,
                DepartureDate = c.DepartureDate,
                EndDate = c.EndDate,
                Days = c.Days,
                People = c.People,
                TotalAmount = c.TotalAmount,
                Status = c.Status,
                Note = c.Note
            }).ToListAsync();
            return Ok(CustomTravelDto);
        }
    }
}
