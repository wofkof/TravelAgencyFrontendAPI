namespace TravelAgency.Shared.Models
{
    public enum ApplicationType
    {
        Passport,
        Visa
    }
    public enum CaseType
    {
        General,
        Urgent
    }
    public class DocumentApplicationForm
    {
        public int ApplicationId { get; set; }

        public int MemberId { get; set; }
        public int? RegionId { get; set; }

        public ApplicationType ApplicationType { get; set; } 
        public string ProcessingItem { get; set; } = null!;
        public CaseType CaseType { get; set; } 
        public byte ProcessingDays { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? StayDuration { get; set; }
        public decimal Fee { get; set; }

        public Member Member { get; set; } = null!;
        public Region? Region { get; set; }
    }
}
