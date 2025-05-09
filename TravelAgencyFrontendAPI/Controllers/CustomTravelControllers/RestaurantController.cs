using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs.CustomTravelDTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelAgencyFrontendAPI.Controllers.CustomTravelControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly AppDbContext _context;
        public RestaurantController(AppDbContext context)
        {
            _context = context;
        }
        //GET: api/Restaurant
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetRestaurant()
        {
            var RestaurantDto = await _context.Restaurants
                .Select(c => new RestaurantDto
                {
                    RestaurantId = c.RestaurantId,
                    DistrictId = c.DistrictId,
                    RestaurantName = c.RestaurantName
                }).ToListAsync();
            return Ok(RestaurantDto);
        }
    }
}
