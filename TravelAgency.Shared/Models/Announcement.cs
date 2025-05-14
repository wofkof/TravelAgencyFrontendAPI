namespace TravelAgency.Shared.Models
{
    public enum AnnouncementStatus
    {
        Published,
        Deleted,
        Archived
    }

    public class Announcement
    {
        public int AnnouncementId { get; set; }
        public int EmployeeId { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public AnnouncementStatus Status { get; set; }

        public Employee Employee { get; set; }
    }

}
