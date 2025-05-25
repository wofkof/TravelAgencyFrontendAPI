using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelAgency.Shared.Models
{
    public enum ApplicationTypeEnum
    {
        passport,
        visa
    }

    public enum CaseTypeEnum
    {
        general,
        urgent
    }
    public class DocumentMenu
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
