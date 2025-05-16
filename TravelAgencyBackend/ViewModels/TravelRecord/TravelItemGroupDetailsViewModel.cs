using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.ViewModels.Order; // 需要 OrderSummaryViewModel

namespace TravelAgencyBackend.ViewModels.TravelRecord
{
    public class TravelItemGroupDetailsViewModel
    {
        // 行程基本資訊
        public int ItemId { get; set; }
        public ProductCategory Category { get; set; }

        [Display(Name = "行程名稱")]
        public string ItemName { get; set; } = string.Empty;

        [Display(Name = "總人數")]
        public int TotalParticipants { get; set; }

        [Display(Name = "總金額")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "訂單數量")]
        public int OrderCount { get; set; }

        // 屬於這個行程群組的已付款訂單列表
        [Display(Name = "相關訂單列表")]
        public List<OrderSummaryViewModel> CompletedOrders { get; set; } = new List<OrderSummaryViewModel>();
    }
}
