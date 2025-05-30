using System.ComponentModel.DataAnnotations;
using System.Reflection;
using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class TravelerProfileDto
    {
        public int? FavoriteTravelerIdToUpdate { get; set; } // 如果是更新現有常用旅客，則提供其 ID；新增則為 null

        [Required(ErrorMessage = "旅客姓名為必填")]
        public string Name { get; set; }

        [Required(ErrorMessage = "旅客生日為必填")]
        public DateTime BirthDate { get; set; }

        public string IdNumber { get; set; } // 身分證號 (台灣)

        public GenderType Gender { get; set; } // 性別 (假設 Gender 為枚舉)

        // public string Phone { get; set; }
        // public string Email { get; set; }

        [Required(ErrorMessage = "旅客證件類型為必填")]
        public DocumentType? DocumentType { get; set; } // 證件類型 (假設 DocumentType 為枚舉)

        public string DocumentNumber { get; set; } // 證件號碼 (護照、居留證等)

        public string PassportSurname { get; set; } // 護照姓

        public string PassportGivenName { get; set; } // 護照名

        public DateTime? PassportExpireDate { get; set; } // 護照效期

        [Required(ErrorMessage = "旅客國籍為必填")]
        public string Nationality { get; set; } // 國籍代碼，例如 "TW", "US"

    }
}
