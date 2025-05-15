using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TravelAgency.Shared.Models;


namespace TravelAgencyBackend.ViewModels // ← 修改成正確拼法
{
    public enum TravelStatus
    {
        [Display(Name = "未上架")]
        Draft,
        [Display(Name = "上架中")]
        Published,
        [Display(Name = "已下架")]
        Unavailable
    }

    public class OfficialTravelViewModel
    {
        public int OfficialTravelId { get; set; }
        [Display(Name = "負責人")]
        public int CreatedByEmployeeId { get; set; }
        [Display(Name = "地區/縣市")]
        public int RegionId { get; set; }

        [Required]
        [Display(Name = "專案名稱")]
        public string Title { get; set; } = null!;

        [Display(Name = "專案年分")]
        public int ProjectYear { get; set; }
        [Display(Name = "上架日期")]
        public DateTime AvailableFrom { get; set; }
        [Display(Name = "下架日期")]
        public DateTime AvailableUntil { get; set; }

        [Required]
        [Display(Name = "專案描述")]
        public string Description { get; set; } = null!;

        [Display(Name = "行程時長")]
        public int Days { get; set; }



        [Display(Name = "封面圖片")]
        [Required(ErrorMessage = "請選擇封面圖片")]
        public IFormFile? CoverImage { get; set; }
      
        [Display(Name = "創建日期")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "最新更新")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "狀態")]
        public TravelStatus Status { get; set; }

        public ICollection<OfficialTravelDetail> Details { get; set; } = new List<OfficialTravelDetail>();

        
    }
}
