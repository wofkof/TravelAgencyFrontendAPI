using System.Reflection;

namespace TravelAgencyFrontendAPI.Models
{
    public class Participant
    {
        public int ParticipantId { get; set; }
        public int MemberId { get; set; }

        public string Name { get; set; }
        public string Phone { get; set; }
        public string IdNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string EnglishName { get; set; }
        public string PassportNumber { get; set; }
        public string IssuedPlace { get; set; }
        public DateTime PassportIssueDate { get; set; }

        public Member Member { get; set; } 
        public ICollection<Order> Orders { get; set; } = new List<Order>();

    }
}
