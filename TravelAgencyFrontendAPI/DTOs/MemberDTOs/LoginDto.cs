using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.MemberDTOs
{
	public class LoginDto
	{
        public string Account { get; set; }  // �i�H��J Email �� ���
        public string Password { get; set; }
    }
}
