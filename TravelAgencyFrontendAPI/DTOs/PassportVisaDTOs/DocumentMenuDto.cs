using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.PassportVisaDTOs
{
    public class DocumentMenuDto
    {
        public int MenuId { get; set; }

        public string? RocPassportOption { get; set; }

        public string? ForeignVisaOption { get; set; }

        public ApplicationTypeEnum ApplicationType { get; set; }

        public string? ProcessingItem { get; set; }

        public CaseTypeEnum? CaseType { get; set; }

        public string? ProcessingDays { get; set; }

        public string? DocumentValidityPeriod { get; set; }

        public string? StayDuration { get; set; }

        public string? Fee { get; set; }
    }
}
