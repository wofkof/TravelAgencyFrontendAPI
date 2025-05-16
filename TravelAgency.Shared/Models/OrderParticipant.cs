namespace TravelAgency.Shared.Models
{
    public class OrderParticipant
    {
        public int OrderParticipantId { get; set; }

        public int OrderId { get; set; }

        public string Name { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public string? IdNumber { get; set; }
        public GenderType Gender { get; set; }
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DocumentType DocumentType { get; set; }
        public string? DocumentNumber { get; set; }
        public string? PassportSurname { get; set; }
        public string? PassportGivenName { get; set; }
        public DateTime? PassportExpireDate { get; set; }
        public string? Nationality { get; set; }
        public string? Note { get; set; }

        public Order Order { get; set; }
        
    }

}
