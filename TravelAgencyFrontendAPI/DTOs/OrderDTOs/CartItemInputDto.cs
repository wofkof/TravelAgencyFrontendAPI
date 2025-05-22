using System.ComponentModel.DataAnnotations;

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class CartItemInputDto
    {
        [Required(ErrorMessage = "必須提供商品類型")]
        public string ProductType { get; set; } //  "GroupTravel", "CustomTravel"

        [Required(ErrorMessage = "必須提供商品ID")]
        public int ProductId { get; set; } // 對應 GroupTravelId 或 CustomTravelId

        [Required(ErrorMessage = "必須提供選項類型")]
        public string OptionType { get; set; } // 例如："Adult", "Child", "Baby" (對應前端的 option.type)

        //public string Description { get; set; } / /Description 由後端根據 Product.Name 和 OptionType 生成

        [Range(1, int.MaxValue, ErrorMessage = "數量至少為1")]
        public int Quantity { get; set; }

    }
}