namespace TravelAgencyFrontendAPI.DTOs.OrderHistoryDTOs
{
    public class OrderHistoryListItemDto
    {
        public int OrderId { get; set; }
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
