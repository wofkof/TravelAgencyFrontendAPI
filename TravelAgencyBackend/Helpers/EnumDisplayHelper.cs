using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using TravelAgency.Shared.Models;
using static System.Net.Mime.MediaTypeNames;

namespace TravelAgencyBackend.Helpers
{
    public static class EnumDisplayHelper
    {
        public static string GetDisplayName<T>(T enumValue) where T : Enum
        {
            if (_displayMappings.TryGetValue(typeof(T), out var map) &&
                map.TryGetValue(enumValue!.ToString()!, out var name))
            {
                return name;
            }
            return enumValue.ToString()!;
        }

        public static List<SelectListItem> GetSelectList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new SelectListItem
                {
                    //Value = e.ToString(),
                    Value = Convert.ToInt32(e).ToString(),
                    Text = GetDisplayName(e)
                }).ToList();
        }

        private static readonly Dictionary<Type, Dictionary<string, string>> _displayMappings = new()
        {
            [typeof(MemberStatus)] = new()
            {
                ["Active"] = "啟用",
                ["Suspended"] = "停權",
                ["Deleted"] = "已刪除"
            },
            [typeof(GenderType)] = new()
            {
                ["Male"] = "男",
                ["Female"] = "女",
                ["Other"] = "其他"
            },
            [typeof(DocumentType)] = new()
            {
                ["ID_CARD_TW"] = "身分證 (台灣)",
                ["PASSPORT"] = "護照",
                ["ARC"] = "居留證",
                ["ENTRY_PERMIT"] = "入台證"
            },
            [typeof(AnnouncementStatus)] = new()
            {
                ["Published"] = "已發布",
                ["Deleted"] = "已刪除",
                ["Archived"] = "已封存"
            },
            [typeof(CallType)] = new()
            {
                ["audio"] = "語音",
                ["video"] = "視訊"
            },
            [typeof(Status)] = new()
            {
                ["completed"] = "已完成",
                ["missed"] = "未接",
                ["rejected"] = "已拒絕"
            },
            [typeof(CallerType)] = new()
            {
                ["Employee"] = "員工",
                ["Member"] = "會員"
            },
            [typeof(ReceiverType)] = new()
            {
                ["Employee"] = "員工",
                ["Member"] = "會員"
            },
            [typeof(ChatStatus)] = new()
            {
                ["Opened"] = "已開啟",
                ["Closed"] = "已關閉"
            },
            [typeof(CollectType)] = new()
            {
                ["Official"] = "套裝行程",
                ["Custom"] = "客製行程"
            },
            [typeof(CommentType)] = new()
            {
                ["Official"] = "套裝行程",
                ["Custom"] = "客製行程"
            },
            [typeof(CommentStatus)] = new()
            {
                ["Visible"] = "顯示",
                ["Hidden"] = "隱藏",
                ["Deleted"] = "刪除"
            },
            [typeof(CustomTravelStatus)] = new()
            {
                ["Pending"] = "待審核",
                ["Approved"] = "已通過",
                ["Rejected"] = "已退回",
                ["Completed"] = "已完成"
            },
            [typeof(TravelItemCategory)] = new()
            {
                ["Accommodation"] = "住宿",
                ["Attraction"] = "景點",
                ["Restaurant"] = "餐廳",
                ["Transport"] = "交通"
            },
            [typeof(ApplicationType)] = new()
            {
                ["Passport"] = "護照",
                ["Visa"] = "簽證"
            },
            [typeof(CaseType)] = new()
            {
                ["General"] = "一般",
                ["Urgent"] = "急件"
            },
            [typeof(EmployeeStatus)] = new()
            {
                ["Active"] = "在職",
                ["Suspended"] = "停職",
                ["Deleted"] = "離職"
            },
            [typeof(FavoriteStatus)] = new()
            {
                ["Active"] = "收藏中",
                ["Deleted"] = "已移除"
            },
            [typeof(SenderType)] = new()
            {
                ["Employee"] = "員工",
                ["Member"] = "會員"
            },
            [typeof(MessageType)] = new()
            {
                ["text"] = "文字",
                ["emoji"] = "表情符號",
                ["sticker"] = "貼圖",
                ["image"] = "圖片",
                ["audio"] = "語音",
                ["video"] = "視訊"
            },
            [typeof(MediaType)] = new()
            {
                ["image"] = "圖片",
                ["audio"] = "語音",
                ["video"] = "視訊"
            },
            [typeof(TravelCategory)] = new()
            {
                ["Domestic"] = "國內旅遊",
                ["Foreign"] = "國外旅遊",
                ["CruiseShip"] = "郵輪"
            },
            [typeof(TravelStatus)] = new()
            {
                ["Active"] = "上架",
                ["Hidden"] = "隱藏",
                ["Deleted"] = "下架"
            },
            [typeof(DetailState)] = new()
            {
                ["Locked"] = "鎖定",
                ["Deleted"] = "已刪除"
            },
            [typeof(OrderStatus)] = new()
            {
                ["Pending"] = "待付款",
                ["Awaiting"] = "待出團",
                ["Completed"] = "已完成",
                ["Cancelled"] = "已取消"
            },
            [typeof(PaymentMethod)] = new()
            {
                ["CreditCard"] = "信用卡",
                ["BankTransfer"] = "銀行轉帳",
                ["Cash"] = "現金",
                ["Other"] = "其他"
            },
            [typeof(InvoiceOption)] = new()
            {
                ["Personal"] = "個人",
                ["Company"] = "公司"
            },
            [typeof(ProductCategory)] = new()
            {
                ["GroupTravel"] = "套裝行程",
                ["CustomTravel"] = "客製行程"
            },
            [typeof(InvoiceStatus)] = new()
            {
                ["Pending"] = "待開立",
                ["Opened"] = "已開立",
                ["Voided"] = "已作廢"
            },
            [typeof(InvoiceType)] = new()
            {
                ["ElectronicInvoice"] = "電子發票",
                ["Double"] = "二聯式",
                ["Triplet"] = "三聯式"
            },
            [typeof(PickupMethodName)] = new()
            {
                ["SelfPickup"] = "自取",
                ["HomeDelivery"] = "宅配"
            },
            [typeof(SupplierType)] = new()
            {
                ["Accommodation"] = "住宿",
                ["Attraction"] = "景點",
                ["Restaurant"] = "餐廳"
            }
        };
    }
}
