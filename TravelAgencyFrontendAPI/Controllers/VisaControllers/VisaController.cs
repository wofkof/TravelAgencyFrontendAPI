using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.VisaDTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelAgencyFrontendAPI.Controllers.VisaControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VisaController(AppDbContext context)
        {
            _context = context;
        }
        // GET: api/DocumentApplicationForms
        // Retrieves all document application forms.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentApplicationFormResponseDto>>> GetDocumentApplicationForms()
        {
            var forms = await _context.DocumentApplicationForms
                                      .Include(f => f.Region) // Include Region for the response DTO
                                      .Select(f => new DocumentApplicationFormResponseDto
                                      {
                                          ApplicationId = f.ApplicationId, // Changed from Id to ApplicationId
                                          ApplicationType = f.ApplicationType,
                                          ProcessingItem = f.ProcessingItem,
                                          CaseType = f.CaseType,
                                          ProcessingDays = f.ProcessingDays, // Type changed to byte
                                          ExpiryDate = f.ExpiryDate,
                                          StayDuration = f.StayDuration, // Added StayDuration
                                          Fee = f.Fee,
                                          RegionId = f.RegionId, // Added RegionId
                                          Region = f.Region, // Include Region object
                                      })
                                      .ToListAsync();
            return Ok(forms);
        }

        // GET: api/DocumentApplicationForms/5
        // Retrieves a single document application form by ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentApplicationFormResponseDto>> GetDocumentApplicationForm(int id)
        {
            // Changed FindAsync to FirstOrDefaultAsync as ApplicationId is the primary key name now.
            // Also included Region for eager loading.
            var form = await _context.DocumentApplicationForms
                                     .Include(f => f.Region)
                                     .FirstOrDefaultAsync(f => f.ApplicationId == id);

            if (form == null)
            {
                return NotFound(); // Returns 404 if the form is not found
            }

            var formDto = new DocumentApplicationFormResponseDto
            {
                ApplicationId = form.ApplicationId, // Changed from Id to ApplicationId
                ApplicationType = form.ApplicationType,
                ProcessingItem = form.ProcessingItem,
                CaseType = form.CaseType,
                ProcessingDays = form.ProcessingDays, // Type changed to byte
                ExpiryDate = form.ExpiryDate,
                StayDuration = form.StayDuration, // Added StayDuration
                Fee = form.Fee,
                RegionId = form.RegionId, // Added RegionId
                Region = form.Region, // Include Region object
            };

            return Ok(formDto); // Returns 200 OK with the form data
        }
    }
}
