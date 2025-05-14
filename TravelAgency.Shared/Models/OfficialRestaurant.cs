namespace TravelAgency.Shared.Models
{
    public class OfficialRestaurant
    {
        public int RestaurantId { get; set; }
        public int? TravelSupplierId { get; set; }
        public int? RegionId { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }

        public TravelSupplier? TravelSupplier { get; set; }
        public Region? Region { get; set; }
    }

}
