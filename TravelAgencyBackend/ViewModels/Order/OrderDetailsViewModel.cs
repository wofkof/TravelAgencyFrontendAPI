using System;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels.Order
{
    public class OrderDetailsViewModel
    {
        [Display(Name = "訂單編號")]
        public int OrderId { get; set; }

        [Display(Name = "會員")] // 顯示會員名稱，標籤用 "會員"
        public int MemberId { get; set; }

        [Display(Name = "會員名稱")] // 或直接用 "會員"
        public string MemberName { get; set; }

        // [Display(Name = "項目編號")] // 可以保留或移除 ItemId 的顯示
        // public int ItemId { get; set; }

        [Display(Name = "訂單內容")] // 新增 ItemName 屬性來顯示名稱
        public string ItemName { get; set; } = "N/A"; // 提供預設值

        [Display(Name = "類別")]
        public ProductCategory Category { get; set; }

        [Display(Name = "參加者")]
        public int ParticipantId { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "參加人數")]
        public int ParticipantsCount { get; set; }

        [Display(Name = "總金額")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "訂單狀態")]
        public OrderStatus Status { get; set; }

        [Display(Name = "付款方式")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "付款日期")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "備註")]
        public string? Note { get; set; }

        // --- 關聯屬性 ---
        // 這些屬性是為了在 Controller 中方便取得關聯資料，
        // 以及在 View 中顯示 Member.Name 和 Participant.Name
        public Member Member { get; set; }
        public OrderParticipant Participant { get; set; }

        // 這兩個屬性在 ViewModel 中可能不是必要的，
        // 因為 ItemName 已經儲存了需要的名稱。
        // 可以考慮移除，除非 View 中還需要它們的其他資訊。
        // public CustomTravel? CustomTravel { get; set; }
        // public OfficialTravelDetail? OfficialTravelDetail { get; set; }

        // --- 非必要屬性 (因為 ItemName 已加入) ---
        // ItemId 仍然需要，以便 Controller 查詢 ItemName
        public int ItemId { get; set; }
    }
}
