namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    // 子 DTO：用於顯示參與者（旅客）資訊
    public class OrderHistoryParticipantDto
    {
        public string Name { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; } = null!;
        public string? Nationality { get; set; }
        public string? IdNumber { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentNumber { get; set; }
        public string? PassportSurname { get; set; }
        public string? PassportGivenName { get; set; }
        public DateTime? PassportExpireDate { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Note { get; set; }
    }
}
