using System.ComponentModel.DataAnnotations;

namespace TravelAgencyBackend.ViewModels
{
    public class PermissionViewModel
    {
        public int PermissionId { get; set; }

        [Required]
        [Display(Name = "權限名稱")]
        [MaxLength(50)]
        public string PermissionName { get; set; } = null!;

        [Display(Name = "權限說明")]
        [MaxLength(100)]
        public string? Caption { get; set; }
    }
}
