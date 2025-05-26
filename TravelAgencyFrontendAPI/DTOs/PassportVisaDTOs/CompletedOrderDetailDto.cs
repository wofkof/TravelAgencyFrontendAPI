using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.PassportVisaDTOs
{
    public class CompletedOrderDetailDto
    {
        public int CompletedOrderDetailId { get; set; }

        public int DocumentMenuId { get; set; }
        public DocumentMenu DocumentMenu { get; set; } = null!;

        public int OrderFormId { get; set; }
        public OrderForm OrderForm { get; set; } = null!;
    }
}
