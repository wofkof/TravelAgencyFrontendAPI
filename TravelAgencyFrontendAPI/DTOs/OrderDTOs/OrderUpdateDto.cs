using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models; 

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class OrderUpdateDto
    {
        [Required(ErrorMessage = "必須提供訂單總金額")]
        [Range(0.01, double.MaxValue, ErrorMessage = "總金額必須大於0")]
        public decimal TotalAmount { get; set; }

        [StringLength(200, ErrorMessage = "訂單備註過長")]
        public string? OrderNotes { get; set; }

        [Required(ErrorMessage = "訂購人資訊為必填")]
        public OrdererInfoDto OrdererInfo { get; set; } = null!; // 與 OrderCreateDto 共用

        [Required(ErrorMessage = "至少需要一位旅客")]
        [MinLength(1, ErrorMessage = "至少需要一位旅客")]
        public List<OrderParticipantDto> Participants { get; set; } = new List<OrderParticipantDto>(); // 與 OrderCreateDto 共用

        [Required(ErrorMessage = "必須提供購物車商品")]
        [MinLength(1, ErrorMessage = "購物車至少需要一項商品")]
        public List<CartItemInputDto> CartItems { get; set; } = new(); // 與 OrderCreateDto 共用

        public List<TravelerProfileDto>? TravelerProfileActions { get; set; } // 與 OrderCreateDto 共用
    }
}