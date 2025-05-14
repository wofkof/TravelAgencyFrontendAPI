using System;
using System.ComponentModel.DataAnnotations;
using TravelAgencyBackend.Models;

namespace TravelAgencyBackend.ViewModels.TravelRecord
{
    public class TravelRecordSummaryViewModel
    {
        [Display(Name = "紀錄編號")]
        public int TravelRecordId { get; set; }

        [Display(Name = "訂單編號")]
        public int OrderId { get; set; }

        // 從關聯的 Order 取得
        [Display(Name = "會員名稱")]
        public string MemberName { get; set; } = string.Empty;

        // 從關聯的 Order 取得
        [Display(Name = "行程名稱")]
        public string ItemName { get; set; } = string.Empty;

        [Display(Name = "總人數")]
        public int TotalParticipants { get; set; }

        [Display(Name = "總金額")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "紀錄建立時間")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CreatedAt { get; set; }
    }
}
