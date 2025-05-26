namespace TravelAgencyFrontendAPI.DTOs.MemberDTOs
{
    public class ChangePasswordDto
    {
        public int MemberId { get; set; }
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
