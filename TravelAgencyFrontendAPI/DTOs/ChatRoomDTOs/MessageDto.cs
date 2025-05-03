namespace TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public int ChatRoomId { get; set; }
        public string SenderType { get; set; } = null!; 
        public int SenderId { get; set; }
        public string MessageType { get; set; } = "text";
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}
