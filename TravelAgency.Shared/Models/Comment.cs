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

        public int OrderDetailId { get; set; } // 新增：關聯哪筆訂單明細
        public ProductCategory Category { get; set; } // GroupTravel / CustomTravel

        public int Rating { get; set; } // 1 ~ 5
        public string? Content { get; set; }

        public CommentStatus Status { get; set; } = CommentStatus.Visible;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Member Member { get; set; } = null!;
        public OrderDetail OrderDetail { get; set; } = null!;
    }
}
