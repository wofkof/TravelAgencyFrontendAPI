using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelAgencyBackend.Models;

namespace TravelAgencyBackend.ViewModels
{
    // ✅ 聊天室列表用（Index）
    public class ChatRoomViewModel
    {
        public int ChatRoomId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [DisplayName("會員名稱")]
        public string MemberName { get; set; } = null!;

        [DisplayName("建立時間")]
        public DateTime CreatedAt { get; set; }

        [DisplayName("狀態")]
        public ChatStatus Status { get; set; }

        [DisplayName("未讀訊息數")]
        public int UnreadCount { get; set; }
    }

    // ✅ 聊天室建立用（Create）
    public class ChatRoomCreateViewModel
    {
        [Required(ErrorMessage = "請選擇會員")]
        [DisplayName("選擇會員")]
        public int MemberId { get; set; }

        public List<SelectListItem>? MemberList { get; set; }
    }

    // ✅ 聊天室詳細頁用（Details）
    public class ChatRoomDetailViewModel
    {
        public int ChatRoomId { get; set; }

        [DisplayName("會員名稱")]
        public string MemberName { get; set; } = null!;

        [DisplayName("狀態")]
        public ChatStatus Status { get; set; }

        [DisplayName("建立時間")]
        public DateTime CreatedAt { get; set; }

        public List<ChatMessageViewModel> Messages { get; set; } = new();
    }

    // ✅ 單一訊息用（Details、GetMessages）
    public class ChatMessageViewModel
    {
        public string SenderType { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string SentAt { get; set; } = null!;
        public bool IsRead { get; set; }
    }

    // ✅ 發送訊息用（如果你之後改成用 AJAX 發送）
    public class SendMessageViewModel
    {
        [Required]
        public int ChatRoomId { get; set; }

        [Required(ErrorMessage = "請輸入訊息內容")]
        [StringLength(500)]
        public string Content { get; set; } = null!;
    }
}
