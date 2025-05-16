namespace TravelAgency.Shared.Models
{
    public class PickupInformation
    {
        public int PickupInfoId { get; set; }

        public string City { get; set; } = null!;
        public string District { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string DetailedAddress { get; set; } = null!;
    }
}
