using Microsoft.AspNetCore.Mvc;

namespace TravelAgencyFrontendAPI.ECPay.Models
{
    public class ECPayCallbackViewModel
    {
        // --- 幾乎一定會有的基礎欄位 ---
        [FromForm(Name = "MerchantID")]
        public string MerchantID { get; set; } = null!;

        [FromForm(Name = "MerchantTradeNo")]
        public string MerchantTradeNo { get; set; } = null!;

        [FromForm(Name = "RtnCode")]
        public int RtnCode { get; set; }

        [FromForm(Name = "RtnMsg")]
        public string RtnMsg { get; set; } = null!;

        [FromForm(Name = "TradeNo")]
        public string TradeNo { get; set; } = null!;

        [FromForm(Name = "TradeDate")]
        public string? TradeDate { get; set; }

        [FromForm(Name = "TradeAmt")]
        public string? TradeAmt { get; set; } 

        //[FromForm(Name = "PaymentDate")]
        //public string? PaymentDate { get; set; }

        [FromForm(Name = "PaymentType")]
        public string? PaymentType { get; set; }

        [FromForm(Name = "MerchantTradeDate")]
        public string? MerchantTradeDate { get; set; }

        [FromForm(Name = "TradeDesc")]
        public string? TradeDesc { get; set; }

        [FromForm(Name = "ItemName")]
        public string? ItemName { get; set; }

        [FromForm(Name = "CheckMacValue")]
        public string CheckMacValue { get; set; } = null!;

        [FromForm(Name = "StoreID")]
        public string? StoreID { get; set; }
        // --- 信用卡交易推薦包含的欄位 ---
        [FromForm(Name = "PaymentTypeChargeFee")]
        public string? PaymentTypeChargeFee { get; set; }

        [FromForm(Name = "SimulatePaid")]
        public int? SimulatePaid { get; set; }

        [FromForm(Name = "gwsr")]
        public string? Gwsr { get; set; } // 信用卡授權識別碼

        [FromForm(Name = "process_date")]
        public string? ProcessDate { get; set; } // 收單行處理日期

        [FromForm(Name = "auth_code")]
        public string? AuthCode { get; set; } // 授權碼

        [FromForm(Name = "card4no")]
        public string? Card4no { get; set; } // 信用卡末四碼

        [FromForm(Name = "card6no")]
        public string? Card6no { get; set; } // 信用卡前六碼

        [FromForm(Name = "eci")]
        public string? Eci { get; set; } // 3D驗證結果

        // --- 自訂欄位
        [FromForm(Name = "CustomField1")]
        public string? OrderId { get; set; }
        [FromForm(Name = "CustomField2")]
        public string? Email { get; set; }
        [FromForm(Name = "CustomField3")]
        public string? Phone { get; set; }
        [FromForm(Name = "CustomField4")]
        public string? CustomField4 { get; set; }

        // --- 可能回傳的欄位 (TradeStatus 較常用於訂單查詢API) ---
        [FromForm(Name = "TradeStatus")] // 雖然主要用於查詢，但有時付款結果也可能帶上
        public int? TradeStatus { get; set; } 

        // --- 只用信用卡，則通常不會出現 ---
        // [FromForm(Name = "BankCode")]
        // public string? BankCode { get; set; }

        // [FromForm(Name = "vAccount")]
        // public string? VAccount { get; set; }

        // [FromForm(Name = "ExpireDate")]
        // public string? ExpireDate { get; set; }
    }
}