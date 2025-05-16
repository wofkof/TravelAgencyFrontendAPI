using System;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels.Order
{
    // 用於顯示最基本的訂單資訊
    public class OrderSimpleDetailsViewModel
    {
        [Display(Name = "訂單編號")]
        public int OrderId { get; set; }

        [Display(Name = "會員名稱")]
        public string MemberName { get; set; } = string.Empty;

        [Display(Name = "行程名稱")]
        public string ItemName { get; set; } = string.Empty;

        [Display(Name = "參與人數")]
        public int ParticipantsCount { get; set; }

        [Display(Name = "訂單金額")] // 改為訂單金額，非總金額
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "訂單建立時間")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "訂單狀態")]
        public OrderStatus Status { get; set; }

    }
}
