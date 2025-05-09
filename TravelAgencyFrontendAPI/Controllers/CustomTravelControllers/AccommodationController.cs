using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs.CustomTravelDTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelAgencyFrontendAPI.Controllers.CustomTravelControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccommodationController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AccommodationController(AppDbContext context)
        {
            _context = context;
        }

        //GET: api/Accommodation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccommodationDto>>> GetAccommodation()
        {
            var AccommodationDto = await _context.Accommodations
                .Select(c => new AccommodationDto
                {
                    AccommodationId = c.AccommodationId,
                    DistrictId = c.DistrictId,
                    AccommodationName = c.AccommodationName
                }).ToListAsync();
            return Ok(AccommodationDto);
        }
    }
}
