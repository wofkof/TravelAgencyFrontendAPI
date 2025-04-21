namespace TravelAgencyFrontendAPI.Models
{
    public class ChatRoom
    {
        public int ChatRoomId { get; set; }
        public int EmployeeId { get; set; }
        public int MemberId { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime? LastMessageAt { get; set; }

        public Employee Employee { get; set; }
        public Member Member { get; set; }
    }
}
