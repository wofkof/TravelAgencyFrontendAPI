﻿namespace TravelAgencyFrontendAPI.DTOs.MemberDTOs
{
    public class VerifyEmailCodeDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
