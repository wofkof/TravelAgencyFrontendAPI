using Microsoft.AspNetCore.Mvc;
using TravelAgencyFrontendAPI.Data;
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
        public async Task<IEnumerable<SearchBoxResultDTO>> Search([FromBody] SearchBoxInputDTO searchBoxInput)
        {
            return _context.OfficialTravels
                .Where(t => t.Title.Contains(searchBoxInput.Destination) || t.Description.Contains(searchBoxInput.Destination) && t.Status == TravelStatus.Active)
                .Select(t => new SearchBoxResultDTO
                {
                    Id = t.OfficialTravelId,
                    Title = t.Title,
                    Description = t.Description
                    
                });
        }
    }
}
