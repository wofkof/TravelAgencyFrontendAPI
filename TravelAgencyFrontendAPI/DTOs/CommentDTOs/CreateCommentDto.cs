using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.CommentDTOs
{
    public class CreateCommentDto
    {
        public int MemberId { get; set; }
        public int OrderDetailId { get; set; }
        public ProductCategory Category { get; set; } // GroupTravel / CustomTravel
        public int Rating { get; set; } // 1~5
        public string? Content { get; set; }
    }
}
