using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TravelAgencyBackend.Helpers;
using TravelAgencyBackend.Models;

namespace TravelAgencyBackend.ViewModels.TravelRecord
{
    public class TravelRecordIndexViewModel
    {
        public PaginatedList<TravelRecordSummaryViewModel>? Records { get; set; }

        // --- 搜尋相關 ---
        [Display(Name = "會員名稱")]
        public string? SearchMemberName { get; set; }

        [Display(Name = "行程名稱")]
        public string? SearchItemName { get; set; } // 可選：加入行程名稱搜尋

        // --- 排序相關 ---
        public string? SortField { get; set; }
        public string? SortDirection { get; set; }

        // --- 分頁相關 ---
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public SelectList? PageSizeOptions { get; set; }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
