using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels.Announcement
{
    public class AnnouncementViewModel
    {
        public int AnnouncementId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = null!;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public AnnouncementStatus Status { get; set; }

        public string? EmployeeName { get; set; } // 顯示發佈者
    }
}
