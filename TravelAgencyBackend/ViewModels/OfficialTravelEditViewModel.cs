using System.ComponentModel.DataAnnotations;

namespace TravelAgencyBackend.ViewModels
{
    public class OfficialTravelEditViewModel
    {
        public int OfficialTravelId { get; set; }
        [Display(Name = "負責人")]
        public int CreatedByEmployeeId { get; set; }
        [Display(Name = "地區/縣市")]
        public int RegionId { get; set; }

        [Required]
        [Display(Name = "專案名稱")]
        public string Title { get; set; }
        [Display(Name = "專案年分")]
        public int ProjectYear { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "上架日期")]
        public DateTime AvailableFrom { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "下架日期")]
        public DateTime AvailableUntil { get; set; }

        [Display(Name = "專案描述")]
        public string? Description { get; set; }
        [Display(Name = "行程時長")]
        public int Days { get; set; }

        [Display(Name = "目前封面")]
        public string? CoverPath { get; set; }
        [Display(Name = "創建日期")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "最新更新")]
        public DateTime? UpdatedAt { get; set; }
        [Display(Name = "狀態")]
        public TravelStatus Status { get; set; }
        [Display(Name = "上傳新封面")]
        public IFormFile? Cover { get; set; }
    }
}
