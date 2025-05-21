namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class GetDetails
    {
        public int DetailId { get; set; }
        public int? Number { get; set; }
        public decimal? Adult { get; set; }
        public decimal? Child { get; set; }
        public decimal? Baby { get; set; }

        public List<GetGroupsIds> GetGroupIds { get; set; }
    }

    public class GetGroupsIds
    {
        public int GroupTravelId { get; set; }
    }
}
