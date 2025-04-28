using System.Xml.Linq;

namespace TravelAgencyFrontendAPI.Models
{
    public enum MemberStatus
    {
        Active,
        Suspended,
        Deleted
    }
    public enum GenderType 
    {
        Male,
        Female,
        Other
    }
    public enum DocumentType
    {
        Passport,
        ResidencePermit,
        EntryPermit
    }
    public class Member
    {
        public int MemberId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime? Birthday { get; set; }
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public GenderType Gender { get; set; }
        public string? IdNumber { get; set; } 
        public string? PassportSurname { get; set; }
        public string? PassportGivenName { get; set; }
        public DateTime? PassportExpireDate { get; set; }
        public string? Nationality { get; set; }
        public DocumentType DocumentType { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Address { get; set; }
        public string PasswordHash { get; set; } = null!;
        public string PasswordSalt { get; set; } = null!;
        public string? GoogleId { get; set; }
        public DateTime RegisterDate { get; set; }
        public MemberStatus Status { get; set; }
        public string? RememberToken { get; set; }
        public DateTime? RememberExpireTime { get; set; }
        public bool IsBlacklisted { get; set; }
        public string? Note { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<MemberFavoriteTraveler> MemberFavoriteTravelers { get; set; } = new List<MemberFavoriteTraveler>();

    }
}
