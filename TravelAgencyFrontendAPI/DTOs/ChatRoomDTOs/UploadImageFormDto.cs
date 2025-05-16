using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs
{
    public class UploadImageFormDto
    {
        public int ChatRoomId { get; set; }
        public int SenderId { get; set; }
        public SenderType SenderType { get; set; } 
        public MessageType MessageType { get; set; } 
        public IFormFile File { get; set; } = null!;
    }
}
