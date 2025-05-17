using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.MemberDTOs
{
	public class SignUpDto
	{
        [Required(ErrorMessage = "Required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        public string Password { get; set; } = string.Empty;

        public string? EmailVerificationCode { get; set; }

    }
}
