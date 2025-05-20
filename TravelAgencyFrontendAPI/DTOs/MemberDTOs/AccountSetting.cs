namespace TravelAgencyFrontendAPI.DTOs.MemberDTOs
{
    public class AccountSetting
    {
        public int MemberId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime? Birthday { get; set; }
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? IdNumber { get; set; }
        public string? Address { get; set; }
        public string? Nationality { get; set; }
        public string? Gender { get; set; }  // "男"、"女"、"其他"
        public string? PassportSurname { get; set; }
        public string? PassportGivenName { get; set; }
        public DateTime? PassportExpireDate { get; set; }
        public string? DocumentType { get; set; }  // e.g., "Passport"
        public string? DocumentNumber { get; set; }
        public string? ProfileImage { get; set; }
        public string? Note { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}
