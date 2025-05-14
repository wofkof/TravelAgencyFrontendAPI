using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelAgencyBackend.Helpers;
using TravelAgency.Shared.Data;
using TravelAgencyBackend.ViewModels.TravelRecord;
using TravelAgencyBackend.ViewModels.Order;
using TravelAgencyBackend.Services;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.Controllers
{
    // Controller 用於顯示行程統計摘要與相關訂單
    public class TravelRecordController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly PermissionCheckService _perm;

        public TravelRecordController(AppDbContext context, PermissionCheckService perm)
            : base(perm)
        {
            _context = context;
            _perm = perm;
        }

        // GET: TravelRecord (顯示行程統計列表)
        public async Task<IActionResult> Index(
            string? searchItemName,
            ProductCategory? searchCategory, // <--- 加入類別搜尋參數
            string? sortField,
            string? sortDirection,
            int? pageNumber,
            int? pageSize)
        {
            var check = CheckPermissionOrForbid("查看訂單");
            if (check != null) return check;

            sortField = string.IsNullOrEmpty(sortField) ? "ItemName" : sortField;
            sortDirection = string.IsNullOrEmpty(sortDirection) ? "asc" : sortDirection;
            int currentPageSize = pageSize ?? 10;
            int currentPageNumber = pageNumber ?? 1;

            // --- 準備 ViewModel ---
            var viewModel = new TravelItemGroupIndexViewModel
            {
                SearchItemName = searchItemName,
                SearchCategory = searchCategory, // <--- 傳遞搜尋條件
                SortField = sortField,
                SortDirection = sortDirection,
                PageIndex = currentPageNumber,
                PageSize = currentPageSize,
                // 準備類別下拉選單
                Categories = new SelectList(Enum.GetValues(typeof(ProductCategory)).Cast<ProductCategory>().Select(c => new SelectListItem
                {
                    Value = ((int)c).ToString(),
                    Text = GetOrderCategoryDisplayName(c) // 使用之前的輔助方法
                }), "Value", "Text", searchCategory), // <--- 加入 SelectList
                PageSizeOptions = new SelectList(new[] { 10, 25, 50, 100 }.Select(s => new { Value = s, Text = $"每頁 {s} 筆" }), "Value", "Text", currentPageSize)
            };

            // --- 查詢並分組訂單 ---
            var paidOrdersQuery = _context.Orders
                                        .Where(o => o.Status == OrderStatus.Completed);

            // 柏亦
            var paidOrderDetailsQuery = _context.OrderDetails
                                               .Where(od => od.Order.Status == OrderStatus.Completed);

            // --- 加入類別篩選 ---
            if (searchCategory.HasValue)
            {
                paidOrderDetailsQuery = paidOrderDetailsQuery.Where(o => o.Category == searchCategory.Value);
            }
            // --- 篩選結束 ---

            var groupedQuery = paidOrderDetailsQuery
                                .GroupBy(o => new { o.ItemId, o.Category })
                                .Select(g => new
                                {
                                    g.Key.ItemId,
                                    g.Key.Category,
                                    TotalParticipants = g.Sum(o => o.Quantity),
                                    TotalAmount = g.Sum(o => o.TotalAmount),
                                    OrderCount = g.Count()
                                });

            var groupedResults = await groupedQuery.ToListAsync();

            var itemGroups = new List<TravelItemGroupViewModel>();
            foreach (var group in groupedResults)
            {
                itemGroups.Add(new TravelItemGroupViewModel
                {
                    ItemId = group.ItemId,
                    Category = group.Category,
                    ItemName = await GetItemName(group.ItemId, group.Category),
                    TotalParticipants = group.TotalParticipants,
                    TotalAmount = group.TotalAmount,
                    OrderCount = group.OrderCount
                });
            }

            // --- 篩選 (行程名稱) ---
            if (!string.IsNullOrEmpty(searchItemName))
            {
                itemGroups = itemGroups.Where(g => g.ItemName.Contains(searchItemName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // --- 排序 ---
            switch (sortField)
            {
                case "TotalParticipants": itemGroups = sortDirection == "desc" ? itemGroups.OrderByDescending(g => g.TotalParticipants).ToList() : itemGroups.OrderBy(g => g.TotalParticipants).ToList(); break;
                case "TotalAmount": itemGroups = sortDirection == "desc" ? itemGroups.OrderByDescending(g => g.TotalAmount).ToList() : itemGroups.OrderBy(g => g.TotalAmount).ToList(); break;
                case "OrderCount": itemGroups = sortDirection == "desc" ? itemGroups.OrderByDescending(g => g.OrderCount).ToList() : itemGroups.OrderBy(g => g.OrderCount).ToList(); break;
                case "ItemName": default: itemGroups = sortDirection == "desc" ? itemGroups.OrderByDescending(g => g.ItemName).ToList() : itemGroups.OrderBy(g => g.ItemName).ToList(); sortField = "ItemName"; break;
            }
            viewModel.SortField = sortField; // 確保 ViewModel 有最新的排序狀態
            viewModel.SortDirection = sortDirection;

            // --- 分頁 ---
            var paginatedItemGroups = PaginatedList<TravelItemGroupViewModel>.Create(itemGroups, currentPageNumber, currentPageSize);

            viewModel.ItemGroups = paginatedItemGroups;
            viewModel.TotalCount = paginatedItemGroups.TotalCount;
            viewModel.TotalPages = paginatedItemGroups.TotalPages;
            viewModel.PageIndex = paginatedItemGroups.PageIndex;

            // --- AJAX 判斷 ---
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_TravelItemGroupListPartial", viewModel);
            }

            // --- 非 AJAX ---
            ViewData["CurrentSortField"] = sortField;
            ViewData["CurrentSortDirection"] = sortDirection;
            // ... (設定 NextSortDirection ViewData) ...

            return View(viewModel);
        }

        // GetOrderCategoryDisplayName 輔助方法 (同之前 OrderController)
        private string GetOrderCategoryDisplayName(ProductCategory category)
        {
            switch (category)
            {
                case ProductCategory.GroupTravel: return "官方"; // 簡短顯示
                case ProductCategory.CustomTravel: return "客製"; // 簡短顯示
                default: return category.ToString();
            }
        }

        // GetItemName 輔助方法 (同之前)
        private async Task<string> GetItemName(int? itemId, ProductCategory? category)
        {
            // ... (同之前) ...
            if (!itemId.HasValue || !category.HasValue) return "N/A";
            string itemName = "N/A";
            if (category.Value == ProductCategory.GroupTravel) { var t = await _context.OfficialTravels.FindAsync(itemId.Value); itemName = t?.Title ?? "官方行程未找到"; }
            else if (category.Value == ProductCategory.CustomTravel) { var t = await _context.CustomTravels.FindAsync(itemId.Value); itemName = t?.Note ?? "客製化行程未找到"; }
            return itemName;
        }

        // Details Action (同之前)
        [HttpGet("TravelRecord/Details/{itemId}/{category}")]
        public async Task<IActionResult> Details(int itemId, ProductCategory category)
        {
            var check = CheckPermissionOrForbid("查看訂單");
            if (check != null) return check;

            // ... (同之前的 Details 邏輯) ...

            var ordersInGroup = await _context.OrderDetails
                                                .Where(o => o.ItemId == itemId && o.Category == category && o.Order.Status == OrderStatus.Completed)
                                                .Include(o => o.Order.Member)
                                                .OrderByDescending(o => o.CreatedAt)
                                                .ToListAsync();



            var completedOrdersSummary = ordersInGroup.Select(o => new OrderSummaryViewModel
            {
                OrderId = o.OrderId, 
                MemberId = o.Order.MemberId,
                MemberName = o.Order.Member?.Name ?? "N/A",
                ItemId = o.ItemId,
                ItemName = "", 
                ParticipantsCount = o.Quantity,
                TotalAmount = o.TotalAmount,
                Status = o.Order.Status,
                CreatedAt = o.CreatedAt
            }).ToList();
            var viewModel = new TravelItemGroupDetailsViewModel
            {
                ItemId = itemId,
                Category = category,
                ItemName = await GetItemName(itemId, category),
                TotalParticipants = ordersInGroup.Sum(o => o.Quantity),
                TotalAmount = ordersInGroup.Sum(o => o.TotalAmount),
                OrderCount = ordersInGroup.Count,
                CompletedOrders = completedOrdersSummary
            };
            foreach (var orderSummary in viewModel.CompletedOrders) { orderSummary.ItemName = viewModel.ItemName; } // 賦值 ItemName

            return View(viewModel);
        }
    }
}
