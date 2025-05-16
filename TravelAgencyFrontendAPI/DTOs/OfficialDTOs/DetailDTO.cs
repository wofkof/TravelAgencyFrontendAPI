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

        //行程總數

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

        public int? AvailableSeats { get; set; }
        //可賣

        public int? TotalSeats { get; set; }
        //席次

    }
}
