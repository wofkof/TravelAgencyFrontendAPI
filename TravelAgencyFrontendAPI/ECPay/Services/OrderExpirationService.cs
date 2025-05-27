using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection; // 需要 IServiceScopeFactory
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TravelAgency.Shared.Data;         
using TravelAgency.Shared.Models;
using Microsoft.EntityFrameworkCore;   // 需要 .ToListAsync() 和 .Where()

namespace TravelAgencyFrontendAPI.Services
{
    public class OrderExpirationService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly ILogger<OrderExpirationService> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // 用於在背景服務中正確解析 Scoped 服務 (如 AppDbContext)

        // --- 設定檢查間隔 ---
        // 測試環境：每 10 秒檢查一次 (如先前討論)
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);
        // 正式環境建議：例如每分鐘檢查一次
        // private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public OrderExpirationService(ILogger<OrderExpirationService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _logger.LogInformation("OrderExpirationService constructor called.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderExpirationService is starting at {Time}.", DateTime.Now); // 使用本地時間記錄啟動時間

            // 設定計時器，在服務啟動後立即開始第一次檢查 (dueTime: TimeSpan.Zero)，之後按 _checkInterval 間隔執行
            _timer = new Timer(DoWork, null, TimeSpan.Zero, _checkInterval);

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("OrderExpirationService is working at {Time}. Checking for expired orders...", DateTime.Now);

            // 因為 AppDbContext 通常是 Scoped 服務，在 IHostedService 的長時間運行方法中，
            // 我們需要為每次操作建立一個新的 Scope，以確保 DbContext 的生命週期正確。
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    var nowUtc = DateTime.UtcNow; // 所有時間比較應使用 UTC
                    var expiredOrders = await dbContext.Orders
                        .Where(o => o.Status == OrderStatus.Awaiting && // 只查找狀態為 Awaiting 的訂單
                                     o.ExpiresAt.HasValue &&           // 確保 ExpiresAt 不是 null
                                     o.ExpiresAt.Value <= nowUtc)      // 且失效時間已到或已過
                        .ToListAsync();

                    if (expiredOrders.Any())
                    {
                        _logger.LogInformation("Found {Count} orders to expire.", expiredOrders.Count);
                        foreach (var order in expiredOrders)
                        {
                            // 再次確認，避免極端情況下重複處理 (雖然機率很小)
                            if (order.Status == OrderStatus.Awaiting)
                            {
                                order.Status = OrderStatus.Expired;
                                // 可選：在備註中添加訂單自動取消的記錄
                                string expireNote = $"訂單因逾期未付款已於 {nowUtc:yyyy-MM-dd HH:mm:ss} UTC 自動取消。";
                                order.Note = string.IsNullOrEmpty(order.Note) ? expireNote : $"{order.Note}\n{expireNote}";

                                _logger.LogInformation("Order {OrderId} (MerchantTradeNo: {MerchantTradeNo}) has expired at {ExpiresAt}. Current UTC time: {NowUtc}. Updating status to Expired.",
                                    order.OrderId, order.MerchantTradeNo, order.ExpiresAt, nowUtc);
                            }
                        }
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation("{Count} orders successfully marked as Expired.", expiredOrders.Count);
                    }
                    else
                    {
                        _logger.LogInformation("No orders found to expire in this run."); // 在沒有訂單過期時也打印日誌，方便監控服務是否正常運行
                    }
                }
                catch (DbUpdateConcurrencyException ex) // 處理並行更新衝突
                {
                    _logger.LogError(ex, "A concurrency error occurred while updating expired orders. Some orders might not have been updated in this run.");
                    // 這裡可以考慮更細緻的錯誤處理，例如標記受影響的訂單以便下次重試
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while processing expired orders in OrderExpirationService.");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderExpirationService is stopping at {Time}.", DateTime.Now);
            _timer?.Change(Timeout.Infinite, 0); // 優雅地停止計時器，不再觸發新的 DoWork
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogInformation("OrderExpirationService is disposing.");
            _timer?.Dispose(); // 釋放計時器資源
        }
    }
}