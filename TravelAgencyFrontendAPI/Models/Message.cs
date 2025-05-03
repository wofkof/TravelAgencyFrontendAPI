namespace TravelAgencyFrontendAPI.Models
{
    public enum SenderType 
    {
        Employee,
        Member 
    }
    public enum MessageType
    {
        text,
        emoji,
        sticker,
        image,
        audio,
        video
    }
    public class Message
    {
        public int MessageId { get; set; }
        public int ChatRoomId { get; set; }

        public SenderType SenderType { get; set; }
        public int SenderId { get; set; }
        public MessageType MessageType { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }

        public ChatRoom ChatRoom { get; set; }
    }
}
