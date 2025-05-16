using System.ComponentModel.DataAnnotations;

namespace TravelAgency.Shared.Models
{
    public enum ChatStatus
    {
        Opened,
        Closed
    }
    public class ChatRoom
    {
        public int ChatRoomId { get; set; }
        public int EmployeeId { get; set; }
        public int MemberId { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public ChatStatus Status { get; set; } = ChatStatus.Opened;

        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public Employee Employee { get; set; }
        public Member Member { get; set; }
    }
}
