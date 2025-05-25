using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs
{
    public class CallLogDto
    {
        public int ChatRoomId { get; set; }
        public CallerType CallerType { get; set; }
        public int CallerId { get; set; }
        public ReceiverType ReceiverType { get; set; }
        public int ReceiverId { get; set; }
        public CallType CallType { get; set; }
        public Status Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DurationInSeconds { get; set; }
    }
}
