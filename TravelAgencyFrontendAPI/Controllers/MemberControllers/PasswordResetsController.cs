using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.Models;
using System.Security.Cryptography;
using System.Text;
using TravelAgencyFrontendAPI.Helpers;

//using Microsoft.AspNetCore.Identity;
using TravelAgencyFrontendAPI.DTOs.MemberDTOs;

namespace TravelAgencyFrontendAPI.Controllers.MemberControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordResetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PasswordResetsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/PasswordResets/request
        [HttpPost("request")]
        public async Task<IActionResult> RequestReset([FromBody] string email)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == email);
            if (member == null)
                return BadRequest("Email 不存在");

            var token = Guid.NewGuid().ToString();
            var reset = new ResetPassword
            {
                MemberId = member.MemberId,
                Token = token,
                CreatedTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddMinutes(30),
                IsUsed = false
            };

            _context.ResetPasswords.Add(reset);
            await _context.SaveChangesAsync();

            // TODO: 寄出 Email（你可以串 SMTP 或用 MailKit 實作）

            return Ok("已寄送重設密碼信");
        }

        // POST: api/PasswordResets/reset
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var reset = await _context.ResetPasswords
                .FirstOrDefaultAsync(r => r.Token == dto.Token && !r.IsUsed && r.ExpireTime > DateTime.Now);

            if (reset == null)
                return BadRequest("重設連結無效或已過期");

            var member = await _context.Members.FindAsync(reset.MemberId);
            if (member == null)
                return BadRequest("找不到對應會員");

            PasswordHasher.CreatePasswordHash(dto.NewPassword, out string newHash, out string newSalt);
            member.PasswordHash = newHash;
            member.PasswordSalt = newSalt;

            reset.IsUsed = true;
            await _context.SaveChangesAsync();

            return Ok("密碼已重設");
        }
    }
}
