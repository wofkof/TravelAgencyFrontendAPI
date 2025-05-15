using System;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels.TravelRecord
{
    public class TravelRecordDetailsViewModel
    {
        [Display(Name = "紀錄編號")]
        public int TravelRecordId { get; set; }

        [Display(Name = "訂單編號")]
        public int OrderId { get; set; }

        [Display(Name = "總人數")]
        public int TotalParticipants { get; set; }

        [Display(Name = "總金額")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "紀錄建立時間")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}")] // 顯示更詳細時間
        public DateTime CreatedAt { get; set; }

        // --- 從關聯訂單來的資訊 ---
        [Display(Name = "會員名稱")]
        public string MemberName { get; set; } = string.Empty;

        [Display(Name = "行程名稱")]
        public string ItemName { get; set; } = string.Empty;

        [Display(Name = "行程類別")]
        public ProductCategory OrderCategory { get; set; }

        [Display(Name = "訂單建立時間")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}")]
        public DateTime OrderCreatedAt { get; set; }

        [Display(Name = "訂單狀態")]
        public OrderStatus OrderStatus { get; set; }

        [Display(Name = "訂單付款方式")]
        public PaymentMethod OrderPaymentMethod { get; set; }

        [Display(Name = "訂單付款日期")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? OrderPaymentDate { get; set; }

        [Display(Name = "訂單備註")]
        public string? OrderNote { get; set; }

        // 可以考慮加入更多訂單相關資訊...
        // public virtual Order RelatedOrder { get; set; } // 或直接傳遞 Order 物件
    }
}
