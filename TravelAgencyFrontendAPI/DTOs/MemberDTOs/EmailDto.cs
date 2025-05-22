using System.Text.Json.Serialization;

namespace TravelAgencyFrontendAPI.DTOs.MemberDTOs
{
    public class EmailDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }
}
