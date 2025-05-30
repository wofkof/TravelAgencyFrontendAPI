namespace TravelAgency.Shared.Models
{
    public enum OrderStatus
    {
        Awaiting, //待付款
        Completed, //付款完成
        Cancelled, //取消付款or失敗
        InvoiceFailed, //付款成功發票開立失敗
        Expired,     // 逾期未付自動失效
    }
    public enum PaymentMethod
    {
        ECPay_CreditCard = 0,
        LinePay = 1,
        Other,
    }
    public enum InvoiceOption
    {
        Personal,
        Company
    }
    public class Order
    {
        public int OrderId { get; set; }
        public int MemberId { get; set; }

        public decimal TotalAmount { get; set; }  //含稅總金額
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.ECPay_CreditCard;
        public OrderStatus Status { get; set; } = OrderStatus.Awaiting;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public DateTime? PaymentDate { get; set; }
        public string? InvoiceDeliveryEmail { get; set; }
        public InvoiceOption InvoiceOption { get; set; } = InvoiceOption.Personal;
        public string? InvoiceUniformNumber { get; set; }
        public string? InvoiceTitle { get; set; }
        public bool InvoiceAddBillingAddr { get; set; } = false;
        public string? InvoiceBillingAddress { get; set; }
        public string? Note { get; set; }
        public string OrdererName { get; set; } = null!;
        public string OrdererPhone { get; set; } = null!;
        public string OrdererEmail { get; set; } = null!;

        public string OrdererNationality { get; set; } = null!;
        public string OrdererDocumentType { get; set; } = null!;
        public string OrdererDocumentNumber { get; set; } = null!;
        public string? ECPayTradeNo { get; set; } // 綠界交易編號
        public string? MerchantTradeNo { get; set; } // 自訂商店交易編號
        public DateTime? ExpiresAt { get; set; }

        public Member Member { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<OrderInvoice> OrderInvoices { get; set; } = new List<OrderInvoice>();
        public ICollection<OrderParticipant> OrderParticipants { get; set; } = new List<OrderParticipant>();
    }
}
