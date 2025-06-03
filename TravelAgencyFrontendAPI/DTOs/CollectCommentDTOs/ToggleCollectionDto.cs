using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.CollectCommentDTOs
{
    public class ToggleCollectionDto
    {
        public int MemberId { get; set; }
        public int TravelId { get; set; }
        public CollectType TravelType { get; set; }
    }
}
