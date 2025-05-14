using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TravelAgencyBackend.ViewModels
{
    public class RoleViewModel
    {
        public int RoleId { get; set; }

        [Required]
        [Display(Name = "角色名稱")]
        [MaxLength(50)]
        public string RoleName { get; set; } = null!;
    }
}
