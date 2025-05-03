namespace TravelAgencyFrontendAPI.DTOs
{
    public class ChatRoomDto
    {
        public int ChatRoomId { get; set; }
        public int EmployeeId { get; set; }
        public int MemberId { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }
}
