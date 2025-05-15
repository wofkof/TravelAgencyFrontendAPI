namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs.Search
{
    public class SearchBoxResultDTO
    {
        public int Id { get; set; } // 旅遊行程的唯一識別碼
        public string Title { get; set; } // 旅遊行程的名稱
        public string Description { get; set; } // 旅遊行程的描述
        //public decimal? Price { get; set; } // 旅遊行程的價格
        //public string? Cover { get; set; }
    }
}
