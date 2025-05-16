using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.Helpers;  

namespace TravelAgencyBackend.ViewModels.TravelRecord
{
    public class TravelItemGroupIndexViewModel
    {
        public PaginatedList<TravelItemGroupViewModel>? ItemGroups { get; set; }

        // --- 搜尋相關 ---
        [Display(Name = "行程名稱")]
        public string? SearchItemName { get; set; }

        [Display(Name = "行程類別")] // <--- 新增
        public ProductCategory? SearchCategory { get; set; }

        public SelectList? Categories { get; set; } // <--- 新增 (給下拉選單用)

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
