using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels
{
    // ➕ 建立參與人用
    public class ParticipantCreateViewModel
    {
        [DisplayName("會員編號")]
        [Required]
        public int MemberId { get; set; }

        [DisplayName("參與人姓名")]
        [Required(ErrorMessage = "請輸入參與人姓名")]
        public string Name { get; set; } = null!;

        [DisplayName("身分證")]
        [Required(ErrorMessage = "請輸入身分證")]
        [RegularExpression(@"^[A-Z]{1}\d{9}$", ErrorMessage = "身分證格式不正確")]
        [StringLength(10, MinimumLength = 10)]
        public string IdNumber { get; set; } = null!;

        [DisplayName("手機")]
        [Required(ErrorMessage = "請輸入手機")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼格式不正確")]
        [StringLength(10, MinimumLength = 10)]
        public string Phone { get; set; } = null!;

        [DisplayName("護照號碼")]
        [Required(ErrorMessage = "請輸入護照號碼")]
        [RegularExpression(@"^[A-Z]{1}\d{8}$", ErrorMessage = "護照號碼格式不正確")]
        [StringLength(9, MinimumLength = 9)]
        public string PassportNumber { get; set; } = null!;

        [DisplayName("發照地")]
        [Required(ErrorMessage = "請選擇發照地")]
        public string IssuedPlace { get; set; } = null!;

        [DisplayName("護照效期起日")]
        [Required(ErrorMessage = "請輸入護照效期")]
        public DateTime? PassportIssueDate { get; set; }

        [DisplayName("英文姓名")]
        [Required(ErrorMessage = "請輸入參與人英文姓名")]
        public string EnglishName { get; set; } = null!;

        [DisplayName("生日")]
        [Required(ErrorMessage = "請輸入生日")]
        public DateTime? BirthDate { get; set; }

        [DisplayName("性別")]
        public GenderType Gender { get; set; } = GenderType.Other;

        [DisplayName("備註")]
        public string? Note { get; set; }
    }

    // ✏️ 編輯參與人用
    public class ParticipantEditViewModel
    {
        [Required]
        public int ParticipantId { get; set; }

        [DisplayName("會員編號")]
        [Required]
        public int MemberId { get; set; }

        [DisplayName("參與人姓名")]
        [Required(ErrorMessage = "請輸入參與人姓名")]
        public string Name { get; set; } = null!;

        [DisplayName("身分證")]
        [Required(ErrorMessage = "請輸入身分證")]
        [RegularExpression(@"^[A-Z]{1}\d{9}$", ErrorMessage = "身分證格式不正確")]
        [StringLength(10, MinimumLength = 10)]
        public string IdNumber { get; set; } = null!;

        [DisplayName("手機")]
        [Required(ErrorMessage = "請輸入手機")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼格式不正確")]
        [StringLength(10, MinimumLength = 10)]
        public string Phone { get; set; } = null!;

        [DisplayName("護照號碼")]
        [Required(ErrorMessage = "請輸入參與人護照號碼")]
        [RegularExpression(@"^[A-Z]{1}\d{8}$", ErrorMessage = "護照號碼格式不正確")]
        [StringLength(9, MinimumLength = 9)]
        public string PassportNumber { get; set; } = null!;

        [DisplayName("發照地")]
        [Required(ErrorMessage = "請選擇發照地")]
        public string IssuedPlace { get; set; } = null!;

        [DisplayName("護照效期起日")]
        [Required(ErrorMessage = "請輸入護照效期")]
        public DateTime? PassportIssueDate { get; set; }

        [DisplayName("英文姓名")]
        [Required(ErrorMessage = "請輸入參與人英文姓名")]
        public string EnglishName { get; set; } = null!;

        [DisplayName("生日")]
        [Required(ErrorMessage = "請輸入生日")]
        public DateTime? BirthDate { get; set; }

        [DisplayName("性別")]
        public GenderType Gender { get; set; } = GenderType.Other;

        [DisplayName("備註")]
        public string? Note { get; set; }
    }

    // 🔍 詳細資料顯示用
    public class ParticipantDetailViewModel
    {
        public int ParticipantId { get; set; }

        public int MemberId { get; set; }

        [DisplayName("會員姓名")]
        public string MemberName { get; set; } = null!;

        [DisplayName("姓名")]
        public string Name { get; set; } = null!;

        [DisplayName("身分證字號")]
        public string IdNumber { get; set; } = null!;

        [DisplayName("手機")]
        public string Phone { get; set; } = null!;

        [DisplayName("護照號碼")]
        public string? PassportNumber { get; set; }

        [DisplayName("發照地點")]
        public string? IssuedPlace { get; set; }

        [DisplayName("備註")]
        public string? Note { get; set; }
    }

    // 📋 列表中的每一筆資料
    public class ParticipantListItemViewModel
    {
        public int ParticipantId { get; set; }

        public string Name { get; set; } = null!;

        public string Phone { get; set; } = null!;

        public string IdNumber { get; set; } = null!;

        public string MemberName { get; set; } = null!;

        public string MemberAccount { get; set; } = null!;
    }

    // 📄 列表查詢 + 分頁用
    public class ParticipantIndexViewModel
    {
        public string? SearchText { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }

        public int? FilterMemberId { get; set; }

        public List<Member> Members { get; set; } = new();

        public List<ParticipantListItemViewModel> Participants { get; set; } = new();
    }
}