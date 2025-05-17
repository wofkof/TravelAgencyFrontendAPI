using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TravelAgencyFrontendAPI.Helpers;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;

//using Microsoft.AspNetCore.Identity;
using TravelAgencyFrontendAPI.DTOs.MemberDTOs;

namespace TravelAgencyFrontendAPI.Controllers.MemberControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordResetsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public PasswordResetsController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // POST: api/PasswordResets/request
        [HttpPost("request")]
        public async Task<IActionResult> RequestReset([FromBody] string email)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == email);
            if (member == null)
                return BadRequest("Email 不存在");

            // 產生 6 碼驗證碼
            var code = new Random().Next(100000, 999999).ToString();

            // 建立 Token 與驗證碼
            //var reset = new ResetPassword
            //{
            //    MemberId = member.MemberId,
            //    Token = Guid.NewGuid().ToString(),
            //    Code = code, // ✅ 新增驗證碼欄位
            //    CreatedTime = DateTime.Now,
            //    ExpireTime = DateTime.Now.AddMinutes(10),
            //    IsUsed = false
            //};

            //_context.ResetPasswords.Add(reset);
            await _context.SaveChangesAsync();

            // 寄送驗證碼 Email
            await _emailService.SendEmailAsync(
                email,
                "嶼你同行 - 密碼重設驗證碼",
                $"您好，您的驗證碼為：<b>{code}</b><br>請在 10 分鐘內輸入驗證碼完成密碼重設流程。"
            );


            return Ok("已寄送重設密碼信");
        }

        // POST: api/PasswordResets/reset
        //[HttpPost("reset")]
        //public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        //{
        //    var verification = await _context.EmailVerificationCodes
        //        .FirstOrDefaultAsync(v =>
        //            v.VerificationCode == dto.Token &&
        //            v.VerificationType == EmailVerificationCode.VerificationTypeEnum.ResetPassword &&
        //            !v.IsVerified &&
        //            v.ExpireAt > DateTime.Now);

        //    if (verification == null)
        //        return BadRequest("驗證碼無效或已過期");

        //    var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == verification.Email);
        //    if (member == null)
        //        return BadRequest("找不到對應會員");

        //    // 更新密碼
        //    PasswordHasher.CreatePasswordHash(dto.NewPassword, out string newHash, out string newSalt);
        //    member.PasswordHash = newHash;
        //    member.PasswordSalt = newSalt;

        //    // 標記驗證碼為已使用
        //    verification.IsVerified = true;
        //    await _context.SaveChangesAsync();

        //    return Ok("密碼已重設");
        //}

    }
}
