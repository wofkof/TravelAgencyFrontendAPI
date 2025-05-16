namespace TravelAgency.Shared.Models
{
    public enum CollectType
    {
        Official,
        Custom
    }

    public class Collect
    {
        public int CollectId { get; set; }

        public int MemberId { get; set; }
        public int TravelId { get; set; }
        public CollectType TravelType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Member Member { get; set; }

    }

}
