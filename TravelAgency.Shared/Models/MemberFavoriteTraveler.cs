using System.Reflection;

namespace TravelAgency.Shared.Models
{
    public enum FavoriteStatus
    {
        Active,
        Deleted
    }
    public class MemberFavoriteTraveler
    {
        public int FavoriteTravelerId { get; set; }
        public int MemberId { get; set; }

        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public string? IdNumber { get; set; } 
        public DateTime? BirthDate { get; set; }
        public GenderType? Gender { get; set; } 
        public string? Email { get; set; }
        public DocumentType? DocumentType { get; set; } 
        public string? DocumentNumber { get; set; }
        public string? PassportSurname { get; set; }
        public string? PassportGivenName { get; set; }
        public DateTime? PassportExpireDate { get; set; }
        public string? Nationality { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Note { get; set; }
        public FavoriteStatus Status { get; set; }
        public string? IssuedPlace { get; set; }

        public Member Member { get; set; }


    }
}
