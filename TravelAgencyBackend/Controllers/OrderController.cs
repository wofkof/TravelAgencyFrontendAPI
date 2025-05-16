using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelAgencyBackend.Helpers;
using TravelAgency.Shared.Data;
using TravelAgencyBackend.Services;
using TravelAgencyBackend.ViewModels.Order;
using TravelAgency.Shared.Models;
using Humanizer;

namespace TravelAgencyBackend.Controllers
{
    public class OrderController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly PermissionCheckService _perm;

        public OrderController(AppDbContext context, PermissionCheckService perm)
            : base(perm) 
        {
            _context = context;
            _perm = perm;
        }

        // GET: Order
        // 加入搜尋和排序參數
        public async Task<IActionResult> Index(
            string? searchMemberName,
            ProductCategory? searchCategory,
            string? sortField,
            string? sortDirection,
            int? pageNumber, // 加入頁碼參數
            int? pageSize)   // 加入每頁筆數參數
        {
            // --- 設定預設值 ---
            var check = CheckPermissionOrForbid("查看訂單");
            if (check != null) return check;

            sortField = string.IsNullOrEmpty(sortField) ? "CreatedAt" : sortField;
            sortDirection = string.IsNullOrEmpty(sortDirection) ? "desc" : sortDirection;
            int currentPageSize = pageSize ?? 10; // 預設每頁 10 筆
            int currentPageNumber = pageNumber ?? 1; // 預設第一頁

            // --- 準備 ViewModel ---
            var viewModel = new OrderIndexViewModel
            {
                SearchMemberName = searchMemberName,
                SearchCategory = searchCategory,
                SortField = sortField,
                SortDirection = sortDirection,
                PageIndex = currentPageNumber,
                PageSize = currentPageSize,
                // 準備類別下拉選單
                Categories = new SelectList(Enum.GetValues(typeof(ProductCategory)).Cast<ProductCategory>().Select(c => new SelectListItem
                {
                    Value = ((int)c).ToString(),
                    Text = GetOrderCategoryDisplayName(c) // 使用輔助方法取得中文名稱
                }), "Value", "Text", searchCategory),
                // 準備每頁筆數選項
                PageSizeOptions = new SelectList(new[] { 10, 25, 50, 100 }.Select(s => new { Value = s, Text = $"每頁 {s} 筆" }), "Value", "Text", currentPageSize)
            };

            // --- 基礎查詢 ---
            var query = _context.OrderDetails.Include(o => o.Order.Member).AsQueryable();

            // --- 篩選 ---
            if (!string.IsNullOrEmpty(searchMemberName))
            {
                query = query.Where(o => o.Order.Member != null && o.Order.Member.Name.Contains(searchMemberName));
            }
            if (searchCategory.HasValue)
            {
                query = query.Where(o => o.Category == searchCategory.Value);
            }

            // --- 排序 ---
            switch (sortField)
            {
                case "ParticipantsCount":
                    query = sortDirection == "desc" ? query.OrderByDescending(o => o.Quantity) : query.OrderBy(o => o.Quantity);
                    break;
                case "TotalAmount":
                    query = sortDirection == "desc" ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount);
                    break;
                case "CreatedAt":
                default:
                    query = sortDirection == "desc" ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt);
                    sortField = "CreatedAt"; // 確保設定
                    break;
            }
            // 更新 ViewModel 中的排序狀態 (給隱藏欄位用)
            viewModel.SortField = sortField;
            viewModel.SortDirection = sortDirection;


            // --- 分頁查詢 (使用 PaginatedList) ---
            // 注意：CreateAsync 會執行 CountAsync 和 Skip/Take/ToListAsync
            var paginatedOrders = await PaginatedList<OrderDetail>.CreateAsync(query, currentPageNumber, currentPageSize);

            // --- 映射到 Summary ViewModel ---
            var orderSummaries = new List<OrderSummaryViewModel>();
            foreach (var o in paginatedOrders) // 只處理當頁資料
            {
                orderSummaries.Add(new OrderSummaryViewModel
                {
                    OrderId = o.OrderId,
                    MemberId = o.Order.MemberId,
                    MemberName = o.Order.Member?.Name ?? "N/A",
                    // 柏亦
                    //ItemId = o.ItemId,
                    // Category = o.Category, // <--- 移除 Category 映射
                    //ParticipantsCount = o.ParticipantsCount,
                    TotalAmount = o.TotalAmount,
                    Status = o.Order.Status,
                    CreatedAt = o.CreatedAt,
                    //ItemName = await GetItemName(o.ItemId, o.Category) // 仍需 Category 來查詢名稱
                });
            }

            // --- 填充 ViewModel 的分頁結果 ---
            viewModel.Orders = new PaginatedList<OrderSummaryViewModel>(orderSummaries, paginatedOrders.TotalCount, paginatedOrders.PageIndex, paginatedOrders.PageSize);
            viewModel.TotalCount = paginatedOrders.TotalCount;
            viewModel.TotalPages = paginatedOrders.TotalPages;
            viewModel.PageIndex = paginatedOrders.PageIndex; // 使用 PaginatedList 計算後的實際頁碼


            // --- AJAX 判斷 ---
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // AJAX 請求回傳包含表格和分頁的 Partial View
                return PartialView("_OrderListPartial", viewModel); // <--- 改用新的 Partial View 名稱
            }

            // --- 非 AJAX 請求 (一般頁面載入) ---
            // 將排序狀態存入 ViewData (維持不變)
            ViewData["CurrentSortField"] = sortField;
            ViewData["CurrentSortDirection"] = sortDirection;
            ViewData["NextSortDirection_ParticipantsCount"] = (sortField == "ParticipantsCount" && sortDirection == "asc") ? "desc" : "asc";
            ViewData["NextSortDirection_TotalAmount"] = (sortField == "TotalAmount" && sortDirection == "asc") ? "desc" : "asc";
            ViewData["NextSortDirection_CreatedAt"] = (sortField == "CreatedAt" && sortDirection == "asc") ? "desc" : "asc";


            return View(viewModel);
        }
        // --- 新增輔助方法：取得訂單類別的中文顯示名稱 ---
        private string GetOrderCategoryDisplayName(ProductCategory category)
        {
            // 根據您的 OrderCategory Enum 定義來回傳中文
            switch (category)
            {
                case ProductCategory.GroupTravel: // 假設 Enum 值是 Official
                    return "官方行程";
                case ProductCategory.CustomTravel:   // 假設 Enum 值是 Custom
                    return "客製化行程";
                // case OrderCategory.OfficialTravelDetail: // 如果您的 Enum 值真的是這個
                //     return "官方行程細節"; // 或其他您想要的中文
                default:
                    return category.ToString(); // 其他未定義的回傳原始名稱
            }
        }

        // GetItemName (同之前)
        private async Task<string> GetItemName(int itemId, ProductCategory category)
        {
            string itemName = "N/A";
            if (category == ProductCategory.GroupTravel)
            {
                var officialTravel = await _context.OfficialTravels.FindAsync(itemId);
                itemName = officialTravel?.Title ?? "官方行程未找到";
            }
            else if (category == ProductCategory.CustomTravel)
            {
                var customTravel = await _context.CustomTravels.FindAsync(itemId);
                itemName = customTravel?.Note ?? "客製化行程未找到";
            }
            return itemName;
        }

        // GET: Order/SimpleDetails/5
        [HttpGet("Order/SimpleDetails/{id}")]
        public async Task<IActionResult> SimpleDetails(int? id, int? originItemId, ProductCategory? originCategory)
        {
            var check = CheckPermissionOrForbid("查看訂單");
            if (check != null) return check;

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Member)
                .Include(o => o.OrderParticipants)
                // .Include(o => o.CustomTravel) // 根據需要決定是否 Include
                // .Include(o => o.OfficialTravelDetail) // 根據需要決定是否 Include
                .FirstOrDefaultAsync(m => m.OrderId == id);

            var orderDeatails = await _context.OrderDetails
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderSimpleDetailsViewModel
            {
                OrderId = order.OrderId,
                MemberName = order.Member?.Name ?? "N/A",
                ItemName = await GetItemName(orderDeatails.ItemId, orderDeatails.Category),
                ParticipantsCount = orderDeatails.Quantity,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Status = order.Status
                // 如果 ViewModel 需要，也可以把 originItemId/Category 加進去
            };

            // --- 將來源行程的 ID 和 Category 存入 ViewBag ---
            if (originItemId.HasValue && originCategory.HasValue)
            {
                ViewBag.OriginItemId = originItemId.Value;
                ViewBag.OriginCategory = originCategory.Value;
                ViewBag.HasOriginInfo = true; // 標記有來源資訊
            }
            else
            {
                ViewBag.HasOriginInfo = false;
            }

            return View(viewModel);
        }

        // --- 保留原本的 Details Action ---
        public async Task<IActionResult> Details(int? id)
        {
            var check = CheckPermissionOrForbid("查看訂單");
            if (check != null) return check;

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Member)
                .Include(o => o.OrderParticipants)
                // .Include(o => o.CustomTravel) // 根據需要決定是否 Include
                // .Include(o => o.OfficialTravelDetail) // 根據需要決定是否 Include
                .FirstOrDefaultAsync(m => m.OrderId == id);

            var orderDeatails = await _context.OrderDetails
                .FirstOrDefaultAsync(m => m.OrderId == id);

            var orderParticipant = await _context.OrderParticipants
                .FirstOrDefaultAsync(op => op.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // 使用 OrderDetailsViewModel
            var orderDetailsViewModel = new OrderDetailsViewModel
            {
                OrderId = order.OrderId,
                MemberId = order.MemberId,
                ItemId = orderDeatails.ItemId,
                Category = orderDeatails.Category,
                ParticipantId = orderParticipant.OrderParticipantId,
                CreatedAt = order.CreatedAt,
                ParticipantsCount = orderDeatails.Quantity,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentMethod = (PaymentMethod)order.PaymentMethod,
                PaymentDate = order.PaymentDate,
                Note = order.Note,
                Member = order.Member, // 傳遞 Member 物件
                Participant = order.OrderParticipants.FirstOrDefault(), // 傳遞 Participant 物件
                ItemName = await GetItemName(orderDeatails.ItemId, orderDeatails.Category) // 加入查詢 ItemName
                // CustomTravel = order.CustomTravel, // 根據需要決定是否傳遞
                // OfficialTravelDetail = order.OfficialTravelDetail // 根據需要決定是否傳遞
            };

            return View(orderDetailsViewModel);
        }

        // GetItemName 輔助方法 (如果 OrderController 沒有，需要從 TravelRecordController 複製或移到共用 Helper)
        private async Task<string> GetItemName(int? itemId, ProductCategory? category)
        {
            if (!itemId.HasValue || !category.HasValue) return "N/A";
            string itemName = "N/A";
            if (category.Value == ProductCategory.GroupTravel) { var t = await _context.OfficialTravels.FindAsync(itemId.Value); itemName = t?.Title ?? "官方行程未找到"; }
            else if (category.Value == ProductCategory.CustomTravel) { var t = await _context.CustomTravels.FindAsync(itemId.Value); itemName = t?.Note ?? "客製化行程未找到"; }
            return itemName;
        }

        // GET: Order/Create
        public IActionResult Create()
        {
            var check = CheckPermissionOrForbid("管理訂單");
            if (check != null) return check;

            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Name");
            ViewData["ParticipantId"] = new SelectList(_context.OrderParticipants, "ParticipantId", "Name");

            // Fetch OfficialTravels and CustomTravels for the dropdowns
            var officialTravels = _context.OfficialTravels.Select(ot => new { Value = ot.OfficialTravelId, Text = ot.Title }).ToList();
            var customTravels = _context.CustomTravels.Select(ct => new { Value = ct.CustomTravelId, Text = ct.Note }).ToList();

            var orderCreateViewModel = new OrderCreateViewModel
            {
                OfficialTravels = new SelectList(officialTravels, "Value", "Text"),
                CustomTravels = new SelectList(customTravels, "Value", "Text")
            };

            return View(orderCreateViewModel);
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel orderCreateViewModel)
        {
            var check = CheckPermissionOrForbid("管理訂單");
            if (check != null) return check;

            // 驗證 ItemId 是否根據 Category 被正確設定 (可以在前端 JS 做，後端也建議驗證)
            if (orderCreateViewModel.Category == ProductCategory.GroupTravel)
            {
                var exists = await _context.OfficialTravels.AnyAsync(ot => ot.OfficialTravelId == orderCreateViewModel.ItemId);
                if (!exists) ModelState.AddModelError("ItemId", "無效的官方行程");
            }
            else if (orderCreateViewModel.Category == ProductCategory.CustomTravel)
            {
                var exists = await _context.CustomTravels.AnyAsync(ct => ct.CustomTravelId == orderCreateViewModel.ItemId);
                if (!exists) ModelState.AddModelError("ItemId", "無效的客製化行程");
            }
            else
            {
                ModelState.AddModelError("Category", "請選擇有效的行程類別");
            }


            if (ModelState.IsValid)
            {
                var order = new Order
                {
                    MemberId = orderCreateViewModel.MemberId,
                    CreatedAt = DateTime.Now, // 確保設定建立時間
                    TotalAmount = orderCreateViewModel.TotalAmount,
                    Status = orderCreateViewModel.Status, // 確保設定初始狀態
                    PaymentMethod = orderCreateViewModel.PaymentMethod,
                    PaymentDate = orderCreateViewModel.PaymentDate,
                    Note = orderCreateViewModel.Note
                };
                _context.Add(order);
                await _context.SaveChangesAsync();

                var orderDetail = new OrderDetail
                {
                    ItemId = orderCreateViewModel.ItemId,
                    Category = orderCreateViewModel.Category,
                    Quantity = orderCreateViewModel.ParticipantsCount,
                };

                _context.Add(orderDetail);
                await _context.SaveChangesAsync();

                var orderParticipant = new OrderParticipant
                {
                    OrderParticipantId = orderCreateViewModel.ParticipantId,
                };

                _context.Add(orderParticipant);
                await _context.SaveChangesAsync();

                // 可以加上成功的提示訊息 (TempData)
                TempData["SuccessMessage"] = "訂單新增成功！";
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is invalid, repopulate the dropdowns
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Name", orderCreateViewModel.MemberId); // 保留已選值
            ViewData["ParticipantId"] = new SelectList(_context.OrderParticipants, "ParticipantId", "Name", orderCreateViewModel.ParticipantId); // 保留已選值

            var officialTravels = _context.OfficialTravels.Select(ot => new { Value = ot.OfficialTravelId, Text = ot.Title }).ToList();
            var customTravels = _context.CustomTravels.Select(ct => new { Value = ct.CustomTravelId, Text = ct.Note }).ToList();

            orderCreateViewModel.OfficialTravels = new SelectList(officialTravels, "Value", "Text", orderCreateViewModel.Category == ProductCategory.GroupTravel ? orderCreateViewModel.ItemId : (int?)null); // 保留已選值
            orderCreateViewModel.CustomTravels = new SelectList(customTravels, "Value", "Text", orderCreateViewModel.Category == ProductCategory.CustomTravel ? orderCreateViewModel.ItemId : (int?)null); // 保留已選值


            return View(orderCreateViewModel);
        }

        // GET: Order/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var check = CheckPermissionOrForbid("管理訂單");
            if (check != null) return check;

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // 取得官方行程和客製化行程的資料，用於填充下拉式選單
            var officialTravels = _context.OfficialTravels.Select(ot => new { Value = ot.OfficialTravelId, Text = ot.Title }).ToList();
            var customTravels = _context.CustomTravels.Select(ct => new { Value = ct.CustomTravelId, Text = ct.Note }).ToList();

            var orderDetails = await _context.OrderDetails
                .FirstOrDefaultAsync(od => od.OrderId == id);

            var orderParticipants = await _context.OrderParticipants
                .FirstOrDefaultAsync(op => op.OrderId == id);

            var orderEditViewModel = new OrderEditViewModel
            {
                OrderId = order.OrderId,
                MemberId = order.MemberId,
                ItemId = orderDetails.ItemId, // 設定初始的 ItemId
                Category = orderDetails.Category,
                ParticipantId = orderParticipants.OrderParticipantId,
                ParticipantsCount = orderDetails.Quantity,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentMethod = (PaymentMethod)order.PaymentMethod,
                PaymentDate = order.PaymentDate,
                Note = order.Note,
                // 根據 Category 設定選中的 ItemId
                OfficialTravels = new SelectList(officialTravels, "Value", "Text", orderDetails.Category == ProductCategory.GroupTravel ? orderDetails.ItemId : (int?)null),
                CustomTravels = new SelectList(customTravels, "Value", "Text", orderDetails.Category == ProductCategory.CustomTravel ? orderDetails.ItemId : (int?)null)
            };

            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Name", order.MemberId); // 設定選中值
            ViewData["ParticipantId"] = new SelectList(_context.OrderParticipants, "ParticipantId", "Name", order.OrderParticipants); // 設定選中值

            return View(orderEditViewModel);
        }

        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderEditViewModel orderEditViewModel)
        {
            var check = CheckPermissionOrForbid("管理訂單");
            if (check != null) return check;

            if (id != orderEditViewModel.OrderId)
            {
                return NotFound();
            }

            // ... (ViewModel 驗證邏輯) ...
            if (orderEditViewModel.Category == ProductCategory.GroupTravel) { /* ... */ }
            else if (orderEditViewModel.Category == ProductCategory.CustomTravel) { /* ... */ }
            else { /* ... */ }


            if (ModelState.IsValid)
            {
                var order = await _context.Orders.FindAsync(id);
                // 柏亦
                var orderDetails = await _context.OrderDetails
                    .FirstOrDefaultAsync(od => od.OrderId == id);
                var orderParticipants = await _context.OrderParticipants
                    .FirstOrDefaultAsync(op => op.OrderId == id);

                if (order == null)
                {
                    return NotFound();
                }

                // 更新 Order 物件的屬性
                order.MemberId = orderEditViewModel.MemberId;
                orderDetails.ItemId = orderEditViewModel.ItemId;
                orderDetails.Category = orderEditViewModel.Category;
                orderParticipants.OrderParticipantId = orderEditViewModel.ParticipantId;
                orderDetails.Quantity = orderEditViewModel.ParticipantsCount;
                order.TotalAmount = orderEditViewModel.TotalAmount;
                order.Status = orderEditViewModel.Status; // 更新狀態
                order.PaymentMethod = orderEditViewModel.PaymentMethod;
                order.PaymentDate = orderEditViewModel.PaymentDate;
                order.Note = orderEditViewModel.Note;

                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync(); // *** 第一次儲存：更新訂單 ***

                    // --- 開始檢查是否要自動新增 TravelRecord ---

                    // 1. 檢查訂單狀態是否為「已付款」 (Paid)
                    if (order.Status == OrderStatus.Completed) // <--- 確認使用 OrderStatus.Paid
                    {
                        // 2. 檢查是否已經為此訂單建立過 TravelRecord
                        bool recordExists = await _context.TravelRecords.AnyAsync(tr => tr.GroupTravelId == order.OrderId);

                        if (!recordExists)
                        {
                            // 3. 如果狀態是「已付款」且紀錄不存在，則建立新的 TravelRecord
                            var newRecord = new TravelRecord
                            {
                                GroupTravelId = order.OrderId,
                                TotalAmount = order.TotalAmount,
                                TotalParticipants = orderDetails.Quantity,
                                CreatedAt = DateTime.Now
                            };

                            _context.TravelRecords.Add(newRecord);
                            await _context.SaveChangesAsync(); // *** 第二次儲存：新增 TravelRecord ***
                        }
                    }
                    // --- 檢查結束 ---

                    TempData["SuccessMessage"] = "訂單更新成功！";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(orderEditViewModel.OrderId)) { return NotFound(); }
                    else { ModelState.AddModelError(string.Empty, "儲存時發生衝突，請重新載入資料。"); }
                    // 重新載入下拉選單等資料
                    ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Name", orderEditViewModel.MemberId);
                    ViewData["ParticipantId"] = new SelectList(_context.OrderParticipants, "ParticipantId", "Name", orderEditViewModel.ParticipantId);
                    // ... (重新載入 OfficialTravels, CustomTravels 下拉選單) ...
                    return View(orderEditViewModel);
                }
                return RedirectToAction(nameof(Index));
            }

            // 如果 ModelState 無效，重新載入下拉選單等資料
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Name", orderEditViewModel.MemberId);
            ViewData["ParticipantId"] = new SelectList(_context.OrderParticipants, "ParticipantId", "Name", orderEditViewModel.ParticipantId);
            // ... (重新載入 OfficialTravels, CustomTravels 下拉選單) ...

            return View(orderEditViewModel);
        }


        // GET: Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var check = CheckPermissionOrForbid("管理訂單");
            if (check != null) return check;

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Member)
                .Include(o => o.OrderParticipants)
                // .Include(o => o.OfficialTravelDetail) // 根據需要 Include
                // .Include(o => o.CustomTravel) // 根據需要 Include
                .FirstOrDefaultAsync(m => m.OrderId == id);

            // 柏亦
            var orderDetails = await _context.OrderDetails
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // 使用 OrderDetailsViewModel
            var orderDetailsViewModel = new OrderDetailsViewModel
            {
                // 柏亦
                OrderId = order.OrderId,
                // MemberId = order.MemberId, // 不需要顯示 ID
                // ItemId = order.ItemId, // 不需要顯示 ID
                Category = orderDetails.Category,
                // ParticipantId = order.ParticipantId, // 不需要顯示 ID
                CreatedAt = order.CreatedAt,
                ParticipantsCount = orderDetails.Quantity,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentMethod = (PaymentMethod)order.PaymentMethod,
                PaymentDate = order.PaymentDate,
                Note = order.Note,
                Member = order.Member, // 傳遞 Member 物件
                Participant = order.OrderParticipants.FirstOrDefault(), // 傳遞 Participant 物件
                ItemName = await GetItemName(orderDetails.ItemId, orderDetails.Category) // 加入查詢 ItemName
                // CustomTravel = order.CustomTravel,
                // OfficialTravelDetail = order.OfficialTravelDetail
            };


            return View(orderDetailsViewModel);
        }


        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "訂單刪除成功！";
            }
            else
            {
                TempData["ErrorMessage"] = "找不到要刪除的訂單。";
            }


            return RedirectToAction(nameof(Index));
        }


        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
