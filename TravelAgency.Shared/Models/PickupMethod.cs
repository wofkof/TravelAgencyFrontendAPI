namespace TravelAgency.Shared.Models
{
    public enum PickupMethodName
    {
        SelfPickup,
        HomeDelivery
    }
    public class PickupMethod
    {
        public byte PickupMethodId { get; set; }
        public PickupMethodName PickupMethodName { get; set; } 
    }
}
