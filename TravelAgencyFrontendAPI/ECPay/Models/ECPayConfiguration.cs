// 路徑：TravelAgencyFrontendAPI/ECPay/Models/ECPayConfiguration.cs

namespace TravelAgencyFrontendAPI.ECPay.Models
{
    public class ECPayConfiguration
    {
        public string MerchantID { get; set; } = null!;
        public string HashKey { get; set; } = null!;
        public string HashIV { get; set; } = null!;
        public string ECPayAioCheckOutUrl { get; set; } = "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5";
        public string ECPayQueryTradeInfoUrl { get; set; } = "https://payment-stage.ecpay.com.tw/Cashier/QueryTradeInfo/V5";
        public string FrontendBaseUrl { get; set; }
        public string FrontendFailureUrl { get; set; }
    }
}
