using System;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels.Order
{
    // 這個 ViewModel 代表 Index 表格中的單一訂單摘要
    public class OrderSummaryViewModel
    {
        [Display(Name = "訂單編號")]
        public int OrderId { get; set; }

        // MemberId 不需要直接顯示，但可以用於關聯
        public int MemberId { get; set; }

        [Display(Name = "會員名稱")]
        public string MemberName { get; set; } = string.Empty; // 直接存放名稱

        // ItemId 不需要直接顯示，但可以用於關聯
        public int ItemId { get; set; }

        [Display(Name = "行程名稱")]
        public string ItemName { get; set; } = string.Empty; // 直接存放行程名稱

        //[Display(Name = "類別")]
        //public OrderCategory Category { get; set; }

        [Display(Name = "參與人數")]
        public int ParticipantsCount { get; set; }

        [Display(Name = "總金額")]
        [DisplayFormat(DataFormatString = "{0:N2}")] // 格式化金額顯示
        public decimal TotalAmount { get; set; }

        [Display(Name = "訂單狀態")]
        public OrderStatus Status { get; set; }

        [Display(Name = "建立日期")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")] // 格式化日期顯示
        public DateTime CreatedAt { get; set; }
    }
}
