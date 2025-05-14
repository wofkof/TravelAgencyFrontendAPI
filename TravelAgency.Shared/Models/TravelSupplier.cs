namespace TravelAgency.Shared.Models
{
    public enum SupplierType
    {
        Accommodation,
        Attraction,
        Restaurant
    }
    public class TravelSupplier
    {
        public int TravelSupplierId { get; set; }
        public string SupplierName { get; set; }
        public SupplierType SupplierType { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string? SupplierNote { get; set; }
    }

}
