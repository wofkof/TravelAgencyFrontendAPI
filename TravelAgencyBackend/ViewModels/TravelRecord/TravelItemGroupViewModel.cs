using System.ComponentModel.DataAnnotations;
using TravelAgencyBackend.Models;

namespace TravelAgencyBackend.ViewModels.TravelRecord
{
    // 代表一個行程 (不論官方或客製) 的統計摘要
    public class TravelItemGroupViewModel
    {
        // 這兩個欄位主要用於識別群組和查詢詳細資料
        public int ItemId { get; set; }
        public OrderCategory Category { get; set; }

        [Display(Name = "行程名稱")]
        public string ItemName { get; set; } = string.Empty;

        [Display(Name = "總人數")]
        public int TotalParticipants { get; set; } // 所有已付款訂單的人數總和

        [Display(Name = "總金額")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalAmount { get; set; } // 所有已付款訂單的金額總和

        [Display(Name = "訂單數量")] // 新增：顯示這個行程有多少筆已付款訂單
        public int OrderCount { get; set; }
    }
}
