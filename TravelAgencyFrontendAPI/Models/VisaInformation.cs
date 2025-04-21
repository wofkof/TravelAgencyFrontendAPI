namespace TravelAgencyFrontendAPI.Models
{
    public class VisaInformation
    {
        public int VisaInfoId { get; set; }
        public int CountryId { get; set; }
        public int VisaTypeId { get; set; }

        public Country Country { get; set; }
        public VisaType VisaType { get; set; }
    }

}
