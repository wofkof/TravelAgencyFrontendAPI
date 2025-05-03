namespace TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs
{
    public class MarkAsReadDto
    {
        public int ChatRoomId { get; set; }
        public int SenderId { get; set; }
        public string SenderType { get; set; } = null!;
    }
}
