using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.CollectCommentDTOs
{
    public class CollectionsBaseDto
    {
        public int MemberId { get; set; }
        public int TravelId { get; set; }
        public int CollectId { get; set; }
        public CollectType TravelType { get; set; }
    }

    public class getMyCollectionsDto : CollectionsBaseDto
    {
        
        public string title { get; set; }
        //客製化行程使用CT Note，官方行程使用OT title
        public string? Description { get; set; }
        //客製化行程為Null
        //聖凱新增
        public string? CoverPath { get; set; }
        public int? ProjectId { get; set; }  
        public int? DetailId { get; set; }
        public int? GroupId { get; set; }
    }

    public class addCollectionDto : CollectionsBaseDto
    {
        
    }

    public class deleteCollectionDto : CollectionsBaseDto
    { 
    
    }

}
