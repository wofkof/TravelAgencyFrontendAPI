using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.CustomTravelDTOs
{
    public class CustomTravelInputDto
    {
        public int CustomTravelId { get; set; }
        public int MemberId { get; set; }
        public int ReviewEmployeeId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public DateTime? DepartureDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Days { get; set; }
        public int People { get; set; }
        public decimal TotalAmount { get; set; }

        public CustomTravelStatus Status { get; set; }
        public string? Note { get; set; }
        public List<ContentInputDto> Contents { get; set; } = new();
    }
}
