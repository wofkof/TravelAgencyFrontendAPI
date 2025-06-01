//using TravelAgency.Shared.Models;

//namespace TravelAgencyFrontendAPI.DTOs.CollectCommentDTOs
//{
//    public class CommentsBaseDto
//    {
//        public int MemberId { get; set; }
//        public int TravelId { get; set; }
//        public int CommentId { get; set; }
//        public CommentType TravelType { get; set; }
//        public int Rating { get; set; }
//        public string? Content { get; set; }
//        public CommentStatus Status { get; set; }
//    }

//    public class getMyCommentsDto : CommentsBaseDto
//    {
//        public string title { get; set; }
//        //客製化行程使用CT Note，官方行程使用OT title
//        public string? Description { get; set; }
//        //客製化行程為Null

//    }


//    public class addMyCommentsDto : CommentsBaseDto
//    {

//    }

//}

using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.CollectCommentDTOs
{
    public class CommentsBaseDto
    {
        public int MemberId { get; set; }
        public int CommentId { get; set; }

        public int OrderDetailId { get; set; } 
        public ProductCategory Category { get; set; }

        public int Rating { get; set; }
        public string? Content { get; set; }
        public CommentStatus Status { get; set; }
    }

    public class getMyCommentsDto : CommentsBaseDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; } 
    }

    public class addMyCommentsDto : CommentsBaseDto
    {
        
    }
}

