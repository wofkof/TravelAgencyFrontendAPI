using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs.CustomTravelDTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelAgencyFrontendAPI.Controllers.CustomTravelControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CityController(AppDbContext context)
        {
            _context = context;
        }

        //GET: api/City
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetCity()
        {
            var CityDto = await _context.Cities
                .Select(c => new CityDto
                {
                    CityId = c.CityId,
                    CityName = c.CityName
                }).ToListAsync();
            return Ok(CityDto);
        }
    }
}
