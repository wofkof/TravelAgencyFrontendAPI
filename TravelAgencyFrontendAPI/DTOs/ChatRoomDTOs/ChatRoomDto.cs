using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs
{
    public class ChatRoomDto
    {
        public int ChatRoomId { get; set; }
        public int EmployeeId { get; set; }
        public int MemberId { get; set; }
        public string? EmployeeName { get; set; }   
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public ChatStatus Status { get; set; }
    }
    public class StartChatRequest
    {
        public int MemberId { get; set; }
    }
}
