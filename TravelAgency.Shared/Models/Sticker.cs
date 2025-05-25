namespace TravelAgency.Shared.Models
{
    public class Sticker
    {
        public int StickerId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }
}
