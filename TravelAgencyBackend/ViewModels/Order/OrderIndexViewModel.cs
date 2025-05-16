using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.Helpers;
using System.ComponentModel.DataAnnotations;

namespace TravelAgencyBackend.ViewModels.Order
{
    public class OrderIndexViewModel
    {
        // 改用 PaginatedList 來儲存當頁訂單
        public PaginatedList<OrderSummaryViewModel>? Orders { get; set; } // 改為 PaginatedList

        // --- 搜尋相關 ---
        [Display(Name = "會員名稱")]
        public string? SearchMemberName { get; set; }

        // SearchCategory 仍然需要，用於 Controller 篩選
        public ProductCategory? SearchCategory { get; set; }
        public SelectList? Categories { get; set; } // 類別下拉選單 (用於搜尋)

        // --- 排序相關 ---
        public string? SortField { get; set; }
        public string? SortDirection { get; set; }

        // --- 分頁相關 ---
        public int PageIndex { get; set; } = 1; // 當前頁碼
        public int PageSize { get; set; } = 10; // 每頁顯示筆數 (預設值)
        public int TotalCount { get; set; } // 總筆數
        public int TotalPages { get; set; } // 總頁數
        public SelectList? PageSizeOptions { get; set; } // 每頁筆數選項

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
