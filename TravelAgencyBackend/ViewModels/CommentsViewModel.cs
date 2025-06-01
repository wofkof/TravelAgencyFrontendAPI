using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels
{
    public class CommentViewModel
    {
        public int CommentId { get; set; }

        // 提交評論時需要的資訊
        public int MemberId { get; set; }
        public int OrderDetailId { get; set; }

        public ProductCategory Category { get; set; } // GroupTravel / CustomTravel
        public int Rating { get; set; } = 5; // 預設 5 顆星
        public string? Content { get; set; }

        // 顯示用
        public string MemberName { get; set; } = string.Empty;
        public string? ProductTitle { get; set; } // 行程名稱（可從 OrderDetail 抓）
        public DateTime CreatedAt { get; set; }

        public CommentStatus Status { get; set; }

        // 是否可編輯（例如時間限制、是否為本人留言）
        public bool CanEdit { get; set; } = false;
    }
}
