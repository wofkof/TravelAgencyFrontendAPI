namespace TravelAgencyFrontendAPI.DTOs.MemberDTOs
{
    public class VerifyCodeDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
