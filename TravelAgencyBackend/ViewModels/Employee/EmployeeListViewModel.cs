using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.Helpers;

namespace TravelAgencyBackend.ViewModels.Employee
{
    public class EmployeeListViewModel
    {
        public int EmployeeId { get; set; }

        [Display(Name = "姓名")]
        public string Name { get; set; } = null!;

        [Display(Name = "性別")]
        public GenderType Gender { get; set; }

        public string GenderDisplay => Gender.GetDisplayName(); // ✅ 中文性別

        [Display(Name = "生日")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "電話")]
        public string Phone { get; set; } = null!;

        [Display(Name = "聯絡信箱")]
        public string Email { get; set; } = null!;

        [Display(Name = "地址")]
        public string? Address { get; set; }

        [Display(Name = "入職日期")]
        public DateTime HireDate { get; set; }

        [Display(Name = "狀態")]
        public EmployeeStatus Status { get; set; }

        public string StatusDisplay => Status.GetDisplayName(); // ✅ 中文狀態

        [Display(Name = "備註")]
        public string? Note { get; set; }

        [Display(Name = "職位")]
        public string RoleName { get; set; } = null!;
    }
}
