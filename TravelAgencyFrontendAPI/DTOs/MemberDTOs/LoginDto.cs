using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.DTOs.MemberDTOs
{
	public class LoginDto
	{
        public string Account { get; set; }  // 可以輸入 Email 或 手機
        public string Password { get; set; }
    }
}
