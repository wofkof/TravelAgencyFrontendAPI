namespace TravelAgencyFrontendAPI.DTOs.OfficialDTOs
{
    public class DetailDTO
    {
        public int ProjectId { get; set; }
        //專案id
        public string Title { get; set; }
        //專案標題

        public string Description { get; set; }
        //專案描述

        public string? Cover { get; set; }
        //封面

        public int DetailId { get; set; }
        //行程id

        public int? Number { get; set; }
        //行程編號

        public decimal? AdultPrice { get; set; }
        //行程成人價格

        public int GroupTravelId { get; set; }
        //出團Id

        public DateTime? DepartureDate { get; set; }
        //出團日期
        public DateTime? ReturnDate { get; set; }
        //回程日期

        public int? AvailableSeats { get; set; }
        //可賣

        public int? TotalSeats { get; set; }
        //席次
        public int? SoldSeats { get; set; }
        //已賣
        public string? GroupStatus { get; set; }

        public int ScheduleId { get; set; }
        //日程
        public string? ScheduleDescription { get; set; }
        public int Day { get; set; }
        //天數
        public string? Breakfast { get; set; }
        //早餐
        public string? Lunch { get; set; }
        //午餐
        public string? Dinner { get; set; }
        //晚餐
        public string? Hotel { get; set; }
        //飯店
        public int? Attraction1 { get; set; }
        //景點1
        public int? Attraction2 { get; set; }
        //景點2
        public int? Attraction3 { get; set; }
        //景點3
        public int? Attraction4 { get; set; }
        //景點4
        public int? Attraction5 { get; set; }
        //景點5

    }
}
