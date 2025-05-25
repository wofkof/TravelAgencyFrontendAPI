// 路徑：TravelAgencyFrontendAPI/ECPay/Models/ECPayConfiguration.cs

using static System.Net.WebRequestMethods;

namespace TravelAgencyFrontendAPI.ECPay.Models
{
    public class ECPayConfiguration
    {
        // 金流相關
        public string MerchantID { get; set; } = null!;
        public string HashKey { get; set; } = null!;
        public string HashIV { get; set; } = null!;
        public string ECPayAioCheckOutUrl { get; set; } = "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5";
        public string ECPayQueryTradeInfoUrl { get; set; } = "https://payment-stage.ecpay.com.tw/Cashier/QueryTradeInfo/V5";

        // 發票相關
        public string Invoice_MerchantID { get; set; } 
        public string Invoice_HashKey { get; set; }    
        public string Invoice_HashIV { get; set; }
        public string Invoice_ApiUrl { get; set; } = "https://einvoice-stage.ecpay.com.tw/B2CInvoice/Issue";


        // 其他共用
        public string FrontendBaseUrl { get; set; } = "https://localhost:3000/";
        public string FrontendFailureUrl { get; set; } = "https://localhost:3000/";
        // public string InvoiceReturnURL { get; set; }
    }
}
