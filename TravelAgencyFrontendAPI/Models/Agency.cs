namespace TravelAgencyFrontendAPI.Models
{
    public class Agency
    {
        public ushort AgencyCode { get; set; }

        public string AgencyName { get; set; } = null!;
        public string ContactPerson { get; set; } = null!;
        public string ContactEmail { get; set; } = null!;
        public string ContactPhone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string ServiceDescription { get; set; } = null!;
    }

}
