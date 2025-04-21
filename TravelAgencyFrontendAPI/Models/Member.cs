namespace TravelAgencyFrontendAPI.Models
{
    public enum MemberStatus
    {
        Active,
        Suspended,
        Deleted
    }
    public class Member
    {
        public int MemberId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime? Birthday { get; set; }
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
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
        public ICollection<Participant> Participants { get; set; } = new List<Participant>();

    }
}
