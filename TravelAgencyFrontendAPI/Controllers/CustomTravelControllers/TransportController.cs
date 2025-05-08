using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs.CustomTravelDTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelAgencyFrontendAPI.Controllers.CustomTravelControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransportController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TransportController(AppDbContext context)
        {
            _context = context;
        }

        //GET: api/Transport
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransportDto>>> GetTransport()
        {
            var TransportDto = await _context.Transports
                .Select(c => new TransportDto
                {
                    TransportId = c.TransportId,
                    TransportMethod = c.TransportMethod
                }).ToListAsync();
            return Ok(TransportDto);
        }
    }
}
