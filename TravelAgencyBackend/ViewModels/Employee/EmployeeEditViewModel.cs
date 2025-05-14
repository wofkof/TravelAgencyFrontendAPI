using System.ComponentModel.DataAnnotations;
using TravelAgencyBackend.Models;

namespace TravelAgencyBackend.ViewModels.Employee
{
    public class EmployeeEditViewModel
    {
        public int EmployeeId { get; set; }

        [Display(Name = "姓名")]
        [Required]
        public string Name { get; set; } = null!;

        [Display(Name = "聯絡信箱")]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "請輸入電話")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼必須為 09 開頭的 10 碼數字")]
        [StringLength(10, MinimumLength = 10)]
        [Display(Name = "帳號(電話)")]
        public string Phone { get; set; } = null!;

        [Display(Name = "生日")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "入職日期")]
        public DateTime HireDate { get; set; }

        [Display(Name = "性別")]
        [Required]
        public GenderType Gender { get; set; }

        [Display(Name = "狀態")]
        [Required]
        public EmployeeStatus Status { get; set; }

        [Display(Name = "地址")]
        public string? Address { get; set; }

        [Display(Name = "備註")]
        public string? Note { get; set; }

        [Display(Name = "職位名稱")]
        [Required]
        public int RoleId { get; set; }

        // 🔐 密碼為 null 代表不修改
        public string? Password { get; set; }

        [Display(Name = "大頭貼")]
        public IFormFile? Photo { get; set; }  // 上傳圖檔

        public string? ImagePath { get; set; }  // 用於顯示原始圖片
    }
}
