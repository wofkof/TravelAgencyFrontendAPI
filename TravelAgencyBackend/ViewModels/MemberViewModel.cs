using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels
{
    // 建立會員用
    public class MemberCreateViewModel
    {
        [Required(ErrorMessage = "請輸入姓名")]
        [DisplayName("姓名")]
        public string Name { get; set; } = null!;


        [Required(ErrorMessage = "請輸入手機")]
        [DisplayName("手機")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼格式不正確，應為 09 開頭共 10 碼")]
        [StringLength(10, MinimumLength = 10)]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "請輸入信箱")]
        [DisplayName("信箱")]
        [EmailAddress(ErrorMessage = "Email格式錯誤")]
        [StringLength(100, ErrorMessage = "信箱長度不可超過 100 字")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "請輸入密碼")]
        [DisplayName("密碼")]
        [StringLength(12, MinimumLength = 6, ErrorMessage = "密碼長度需為 6 到 12 字")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,12}$",
        ErrorMessage = "密碼需包含英文字母與數字，且長度為 6~12 字")]
        public string Password { get; set; } = null!;
        
    }

    // ✏️ 編輯會員用
    public class MemberEditViewModel
    {
        [DisplayName("編號")]
        public int MemberId { get; set; }

        [Required(ErrorMessage = "請輸入姓名")]
        [DisplayName("姓名")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "請輸入信箱")]
        [DisplayName("信箱")]
        [EmailAddress(ErrorMessage = "格式不正確")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "請輸入手機")]
        [DisplayName("手機")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼格式不正確")]
        [StringLength(10, MinimumLength = 10)]
        public string Phone { get; set; } = null!;

        [DisplayName("性別")]
        public GenderType Gender { get; set; }
        [DisplayName("生日")]
        public DateTime? Birthday { get; set; }
        [DisplayName("國籍")]
        public string? Nationality { get; set; }
        [DisplayName("護照姓")]
        public string? PassportSurname { get; set; }
        [DisplayName("護照名")]
        public string? PassportGivenName { get; set; }
        [DisplayName("身分證")]
        [RegularExpression(@"^[A-Z]{1}\d{9}$", ErrorMessage = "身分證格式不正確")]
        public string? IdNumber { get; set; }
        [DisplayName("地址")]
        public string? Address { get; set; }

        [DisplayName("狀態")]
        public MemberStatus Status { get; set; } = MemberStatus.Active;

        [DisplayName("備註")]
        public string? Note { get; set; }
    }

    // 🔍 查看會員用
    public class MemberDetailViewModel
    {
        [DisplayName("編號")]
        public int MemberId { get; set; }

        [DisplayName("帳號")]
        public string Account { get; set; } = null!;

        [DisplayName("姓名")]
        public string Name { get; set; } = null!;

        [DisplayName("信箱")]
        public string Email { get; set; } = null!;

        [DisplayName("手機")]
        public string Phone { get; set; } = null!;
        [DisplayName("性別")]
        public GenderType Gender { get; set; }
        [DisplayName("生日")]
        public DateTime? Birthday { get; set; }
        [DisplayName("國籍")]
        public string? Nationality { get; set; }
        [DisplayName("護照姓")]
        public string? PassportSurname { get; set; }
        [DisplayName("護照名")]
        public string? PassportGivenName { get; set; }
        [DisplayName("身分證")]
        public string? IdNumber { get; set; }
        [DisplayName("地址")]
        public string? Address { get; set; }

        [DisplayName("狀態")]
        public MemberStatus Status { get; set; }

        [DisplayName("註冊時間")]
        public DateTime CreatedAt { get; set; }

        [DisplayName("更新時間")]
        public DateTime? UpdatedAt { get; set; }

        [DisplayName("備註")]
        public string? Note { get; set; }
    }

    // 📋 列表中每一筆用
    public class MemberListItemViewModel
    {
        [DisplayName("會員編號")]
        public int MemberId { get; set; }

        [DisplayName("帳號")]
        public string Account { get; set; } = null!;

        [DisplayName("姓名")]
        public string Name { get; set; } = null!;

        [DisplayName("手機")]
        public string Phone { get; set; } = null!;

        [DisplayName("信箱")]
        public string Email { get; set; } = null!;

        [DisplayName("狀態")]
        public MemberStatus Status { get; set; }
    }

    // 📄 列表頁查詢 & 分頁用
    public class MemberIndexViewModel
    {
        [DisplayName("關鍵字")]
        public string? SearchText { get; set; }

        [DisplayName("狀態篩選")]
        public MemberStatus? FilterStatus { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }

        public List<MemberListItemViewModel> Members { get; set; } = new();
    }
}
