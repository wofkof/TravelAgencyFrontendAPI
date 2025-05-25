namespace TravelAgency.Shared.Models
{
    public enum PaymentMethodEnum
    {
        信用卡,
        行動電子支付
    }
    public class Payment
    {
        public int PaymentId { get; set; }

        public int OrderFormId { get; set; }
        public OrderForm OrderForm { get; set; } = null!;

        public int DocumentMenuId { get; set; }
        public DocumentMenu DocumentMenu { get; set; } = null!;

        public PaymentMethodEnum PaymentMethod { get; set; }
    }
}
 