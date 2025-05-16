using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.FavoriteTravelerDTOs
{
    public class FavoriteTravelerDto
    {
        public int? FavoriteTravelerId { get; set; } // PUT 時必帶，POST 可省略
        public int MemberId { get; set; }

        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public string IdNumber { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public GenderType? Gender { get; set; }
        public string? Email { get; set; }
        public DocumentType? DocumentType { get; set; }
        public string? DocumentNumber { get; set; }
        public string? PassportSurname { get; set; }
        public string? PassportGivenName { get; set; }
        public DateTime? PassportExpireDate { get; set; }
        public string? Nationality { get; set; }
        public string? Note { get; set; }
    }
}
