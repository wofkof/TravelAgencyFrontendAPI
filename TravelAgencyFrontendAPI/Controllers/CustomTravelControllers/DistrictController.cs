using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.DTOs.CustomTravelDTOs;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelAgencyFrontendAPI.Controllers.CustomTravelControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistrictController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DistrictController(AppDbContext context)
        {
            _context = context;
        }

        //GET: api/District
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DistrictDto>>> GetDistrict()
        {
            var DistrictDto = await _context.Districts
                .Select(c => new DistrictDto
                {
                    DistrictId = c.DistrictId,
                    CityId = c.CityId,
                    DistrictName = c.DistrictName
                }).ToListAsync();
            return Ok(DistrictDto);
        }
    }
}
