namespace TravelAgency.Shared.Models
{
    public enum CommentType
    {
        Official,
        Custom
    }

    public enum CommentStatus
    {
        Visible,
        Hidden,
        Deleted
    }
    public class Comment
    {
        public int CommentId { get; set; }

        public int MemberId { get; set; }
        public int TravelId { get; set; }
        public CommentType TravelType { get; set; }
        public int Rating { get; set; } 
        public string? Content { get; set; }

        public CommentStatus Status { get; set; } = CommentStatus.Visible;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Member Member { get; set; }

    }

}
