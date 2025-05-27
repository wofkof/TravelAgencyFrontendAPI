using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.PassportVisaDTOs
{
    public class PaymentDTO
    {
        public int PaymentId { get; set; }

        public int OrderFormId { get; set; }
        public OrderForm OrderForm { get; set; } = null!;

        public int DocumentMenuId { get; set; }
        public DocumentMenu DocumentMenu { get; set; } = null!;

        public PaymentMethodEnum PaymentMethod { get; set; }
    }
}
