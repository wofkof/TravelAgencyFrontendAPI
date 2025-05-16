using System.ComponentModel.DataAnnotations;

namespace TravelAgencyBackend.ViewModels.Login
{
    public class ResetPasswordViewModel
    {
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "此欄位為必填!")]
        [DataType(DataType.Password)]
        [Display(Name = "請輸入新密碼")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "此欄位為必填!")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "兩次輸入的密碼不一致")]
        [Display(Name = "請再次輸入新密碼")]
        public string ConfirmPassword { get; set; }
    }
}
