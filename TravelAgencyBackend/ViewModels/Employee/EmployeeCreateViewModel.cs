using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels.Employee
{
    public class EmployeeCreateViewModel
    {
        [Display(Name = "姓名")]
        [Required(ErrorMessage = "請輸入員工姓名")]
        public string Name { get; set; } = null!;


        [Display(Name = "密碼")]
        [Required(ErrorMessage = "密碼欄位為必填")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;


        [Display(Name = "聯絡信箱")]
        [Required(ErrorMessage = "請輸入聯絡信箱")]
        [EmailAddress(ErrorMessage = "請確認輸入信箱的格式")]
        public string Email { get; set; } = null!;



        [Required(ErrorMessage = "請輸入電話")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼必須為 09 開頭的 10 碼數字")]
        [StringLength(10,MinimumLength =10)]
        [Display(Name = "帳號(電話)")]
        public string Phone { get; set; } = null!;


        [Display(Name = "生日")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "入職日期")]
        public DateTime HireDate { get; set; }

        [Display(Name = "性別")]
        [Required(ErrorMessage = "請選擇性別")]
        public GenderType? Gender { get; set; }

        [Display(Name = "狀態")]
        [Required(ErrorMessage = "請選擇員工狀態")]
        public EmployeeStatus? Status { get; set; }

        [Required(ErrorMessage = "請輸入地址")]
        [Display(Name = "地址")]
        public string? Address { get; set; }

        [Display(Name = "備註")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "請選擇員工職位")]
        [Display(Name = "職位")]
        public int? RoleId { get; set; }

        [Display(Name = "大頭貼")]
        public IFormFile? Photo { get; set; }
        public string? ImagePath { get; set; } // 供顯示用（Edit 用得到）


    }
}
