namespace TravelAgency.Shared.Models
{
    public class EmailVerificationCode
    {
        public enum VerificationTypeEnum
        {
            SignUp,
            ResetPassword
        }

        public int VerificationId { get; set; } 
        public string Email { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
        public VerificationTypeEnum VerificationType { get; set; }

        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireAt { get; set; }
    }
}
