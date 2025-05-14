namespace TravelAgency.Shared.Models
{
    public class ResetPassword
    {
        public int TokenId { get; set; }
        public int MemberId { get; set; }
        public string Token { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ExpireTime { get; set; }
        public bool IsUsed { get; set; }

        public Member Member { get; set; }
    }
}
