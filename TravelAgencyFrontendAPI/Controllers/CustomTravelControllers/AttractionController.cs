using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs.CustomTravelDTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelAgencyFrontendAPI.Controllers.CustomTravelControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttractionController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AttractionController(AppDbContext context)
        {
            _context = context;
        }

        //GET: api/Attraction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttractionDto>>> GetAttraction()
        {
            var AttractionDto = await _context.Attractions
                .Select(c => new AttractionDto
                {
                    AttractionId = c.AttractionId,
                    DistrictId = c.DistrictId,
                    AttractionName = c.AttractionName
                }).ToListAsync();
            return Ok(AttractionDto);
        }
    }
}
