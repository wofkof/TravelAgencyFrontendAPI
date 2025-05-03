namespace TravelAgencyFrontendAPI.Models
{
    public class DocumentOrderDetails
    {
        public int DocumentOrderId { get; set; }

        public int ApplicationId { get; set; }
        public byte PickupMethodId { get; set; }
        public int? PickupInfoId { get; set; }
        public ushort AgencyCode { get; set; }

        public ApplicationType ApplicationType { get; set; } 
        public string RequiredData { get; set; } = null!;
        public string SubmissionMethod { get; set; } = null!;
        public string Notes { get; set; } = null!;

        public DateTime DepartureDate { get; set; }
        public byte ProcessingCount { get; set; }

        public string ChineseLastName { get; set; } = null!;
        public string ChineseFirstName { get; set; } = null!;
        public string EnglishLastName { get; set; } = null!;
        public string EnglishFirstName { get; set; } = null!;
        public DateTime BirthDate { get; set; }

        public DocumentApplicationForm DocumentApplicationForm { get; set; } = null!;
        public PickupMethod PickupMethod { get; set; } = null!;
        public PickupInformation? PickupInformation { get; set; }
        public Agency Agency { get; set; } = null!;
    }

}
