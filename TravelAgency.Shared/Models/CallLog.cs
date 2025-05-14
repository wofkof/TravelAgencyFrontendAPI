namespace TravelAgency.Shared.Models
{
    public enum CallType 
    {
        audio,
        video
    }
    public enum Status 
    {
        completed,
        missed,
        rejected
    }
    public enum CallerType 
    {
        Employee,
        Member
    }
    public enum ReceiverType
    {
        Employee,
        Member
    }
    public class CallLog
    {
        public int CallId { get; set; }
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

        public ChatRoom ChatRoom { get; set; }
    }
}
