namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class SearchInput
    {
        public string Destination { get; set; }   // 可輸入「日本」、「北海道」等
        public int PeopleCount { get; set; } = 1;     // 人數（選填，永遠大於等於1）
        public DateTime? StartDate { get; set; }   // 最早出發日（選填）
        public DateTime? EndDate { get; set; }     // 最晚出發日（選填）
    }
}
