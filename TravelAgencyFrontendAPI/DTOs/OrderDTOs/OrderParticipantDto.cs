// In file: DTOs/OrderDTOs/OrderParticipantDto.cs
using System;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models; // For GenderType, DocumentType enums

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class OrderParticipantDto
    {
        // 如果旅客是從「常用旅客清單」選取的，可以傳入 MemberFavoriteTravelerId
        // 後端可以根據這個ID預填部分資料，但仍應允許前端覆蓋/提供最新資料
        public int? FavoriteTravelerId { get; set; } // 對應 MemberFavoriteTraveler.Id

        // 如果這位旅客就是訂購人(當前登入會員)，可以用一個標誌或讓前端直接填入會員ID
        // 另一種方式是前端傳送該旅客對應的 MemberId (如果是系統內的會員)
        public int? MemberIdAsParticipant { get; set; } // 若此旅客本身也是系統會員，則其 MemberId

        [Required(ErrorMessage = "旅客姓名為必填")]
        [StringLength(100)]
        public string Name { get; set; }

        // BirthDate, IdNumber, Gender 等欄位根據 OrderParticipant Model 加入
        // 這些是實際旅客資料，即使從常用旅客帶入，也應包含在送往後端的資料中
        [Required(ErrorMessage = "旅客生日為必填")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "旅客身分證號/證件號碼為必填")]
        [StringLength(50)]
        public string IdNumber { get; set; } // 根據 DocumentType 可能對應不同證件號碼

        [Required(ErrorMessage = "旅客性別為必填")]
        public GenderType Gender { get; set; }

        //[Required(ErrorMessage = "旅客手機為必填")]
        //[Phone(ErrorMessage = "請輸入有效的手機號碼")]
        [StringLength(20)]
        public string Phone { get; set; }

        //[Required(ErrorMessage = "旅客電子信箱為必填")]
        //[EmailAddress(ErrorMessage = "請輸入有效的電子信箱")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required(ErrorMessage = "旅客證件類型為必填")]
        public DocumentType DocumentType { get; set; } // Passport, ResidencePermit, EntryPermit

        [StringLength(100)]
        public string? DocumentNumber { get; set; } // 實際的證件號碼 (若與IdNumber不同或更具體)

        // 根據 DocumentType == Passport 時的相關欄位 (可選)
        [StringLength(100)]
        public string? PassportSurname { get; set; }
        [StringLength(100)]
        public string? PassportGivenName { get; set; }
        public DateTime? PassportExpireDate { get; set; }
        [StringLength(100)]
        public string? Nationality { get; set; }

        [StringLength(255)]
        public string? Note { get; set; } // 個別旅客的備註
    }
}