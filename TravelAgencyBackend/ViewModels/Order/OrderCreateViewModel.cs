using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels.Order
{
    public class OrderCreateViewModel
    {
        [Required(ErrorMessage = "請選擇會員")] // <--- 加入錯誤訊息
        [Display(Name = "會員")]
        public int MemberId { get; set; }

        // ItemId 的前端驗證比較複雜，主要依賴後端
        [Required(ErrorMessage = "請選擇行程")] // 基本驗證，確保有值被 JS 填入
        [Display(Name = "訂單內容")]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "請選擇訂單類型")]
        [Display(Name = "類別")]
        public ProductCategory Category { get; set; }

        [Required(ErrorMessage = "請選擇參與者")]
        [Display(Name = "參與者")]
        public int ParticipantId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "參與人數至少為 1")] // <--- 確保有錯誤訊息
        [Display(Name = "參與人數")]
        public int ParticipantsCount { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "總金額必須大於 0")] // <--- 確保有錯誤訊息
        [Display(Name = "總金額")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "請選擇狀態")]
        [Display(Name = "狀態")]
        public OrderStatus Status { get; set; }

        [Required(ErrorMessage = "請選擇付款方式")]
        [Display(Name = "付款方式")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "付款日期")]
        // 如果付款日期是必填，加上 [Required]
        // [Required(ErrorMessage = "請選擇付款日期")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "備註")]
        [StringLength(500, ErrorMessage = "備註不能超過 500 字")] // 例如加上長度限制
        public string? Note { get; set; }

        // 下拉選單資料源
        public SelectList? OfficialTravels { get; set; }
        public SelectList? CustomTravels { get; set; }
    }
}
