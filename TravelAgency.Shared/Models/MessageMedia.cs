namespace TravelAgency.Shared.Models
{
    public enum MediaType
    {
        image,
        audio,
        video
    }
    public class MessageMedia
    {
        public int MediaId { get; set; }
        public int MessageId { get; set; }
        public MediaType MediaType { get; set; }
        public string FilePath { get; set; }
        public int? DurationInSeconds { get; set; }

        public Message Message { get; set; }
    }
}
