using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;

namespace TravelAgencyFrontendAPI.DTOs.MemberDTOs
{
	public class SignUpDto
	{
        [Required(ErrorMessage = "Required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Password { get; set; }
    }
}
