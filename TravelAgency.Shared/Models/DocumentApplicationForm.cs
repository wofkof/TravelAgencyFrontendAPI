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

        //public int MemberId { get; set; } 移動到證件訂單明細
        public int? RegionId { get; set; }

        public ApplicationType ApplicationType { get; set; } 
        public string ProcessingItem { get; set; } = null!;
        public CaseType CaseType { get; set; } 
        public byte ProcessingDays { get; set; }

        
        public string ExpiryDate { get; set; }//更改資料庫欄位範例富成

        public string? StayDuration { get; set; }
        public decimal Fee { get; set; }

        //public Member Member { get; set; } = null!;
        public Region? Region { get; set; }
    }
}
