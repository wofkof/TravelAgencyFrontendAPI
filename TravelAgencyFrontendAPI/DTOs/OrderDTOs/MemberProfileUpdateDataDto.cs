using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models; 

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class MemberProfileUpdateDataDto
    {
        [Required(ErrorMessage = "會員姓名為必填")]
        [StringLength(100)]
        public string Name { get; set; } // 會員的完整姓名

        //[Phone(ErrorMessage = "請輸入有效的手機號碼")]
        [StringLength(20)]
        public string MobilePhone { get; set; } // 會員的手機

        [EmailAddress(ErrorMessage = "請輸入有效的電子信箱")]
        [StringLength(255)]
        public string Email { get; set; } // 會員的Email

        [StringLength(10)] // 例如 "TW", "US"
        public string Nationality { get; set; }

        public DocumentType DocumentType { get; set; }

        [StringLength(50)]
        public string DocumentNumber { get; set; }
    }
}