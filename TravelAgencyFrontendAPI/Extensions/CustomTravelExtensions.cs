using System.Collections.Generic;
using TravelAgency.Shared.Models; 

namespace TravelAgencyFrontendAPI.Extensions
{
    public static class CustomTravelExtensions
    {
        private static readonly Dictionary<CustomTravelStatus, string> _statusTexts = new()
        {
            { CustomTravelStatus.Pending, "待審核" },
            { CustomTravelStatus.Approved, "已通過" },
            { CustomTravelStatus.Rejected, "已取消" },
            { CustomTravelStatus.Completed, "已完成" }
        };

        public static string ToChinese(this CustomTravelStatus status)
        {
            return _statusTexts.TryGetValue(status, out var text) ? text : "未知狀態";
        }

        private static readonly Dictionary<TravelItemCategory, string> _CategoryTexts = new()
        {
            { TravelItemCategory.Attraction,"景點"},
            { TravelItemCategory.Restaurant,"餐廳"},
            { TravelItemCategory.Accommodation,"住宿"},
            { TravelItemCategory.Transport,"交通"}
        };

        public static string TurnChinese(this TravelItemCategory category)
        {
            return _CategoryTexts.TryGetValue(category, out var text) ? text : "未知";
        }
    }
}
