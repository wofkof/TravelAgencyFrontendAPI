using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.DTOs.MemberDTOs
{
    public class ResetPasswordDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
