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
                return BadRequest("該會員不存在，請先註冊");

            // 產生 6 碼驗證碼
            var code = new Random().Next(100000, 999999).ToString();

            // 建立一筆 Email 驗證資料
            var verification = new EmailVerificationCode
            {
                Email = email,
                VerificationCode = code,
                VerificationType = EmailVerificationCode.VerificationTypeEnum.ResetPassword,
                IsVerified = false,
                CreatedAt = DateTime.Now,
                ExpireAt = DateTime.Now.AddMinutes(10)
            };

            _context.EmailVerificationCodes.Add(verification);
            await _context.SaveChangesAsync();

            // 發送 Email
            await _emailService.SendEmailAsync(
                email,
                "嶼你同行 - 密碼重設驗證碼",
                $"您好，您的驗證碼為：<b>{code}</b><br>請在 10 分鐘內完成驗證碼輸入與密碼設定。<br><br><span style='font-size:13px;color:#888'>此為系統自動發送，請勿回覆。</span>"
            );

            return Ok("已寄出驗證碼");
        }

        // POST: api/PasswordResets/verify-code
        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto dto)
        {
            var record = await _context.EmailVerificationCodes
                .Where(v => v.Email == dto.Email && v.VerificationType == EmailVerificationCode.VerificationTypeEnum.ResetPassword)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (record == null || record.IsVerified || record.ExpireAt < DateTime.Now)
                return BadRequest("驗證碼無效或已過期");

            if (record.VerificationCode != dto.Code)
                return BadRequest("驗證碼錯誤");

            record.IsVerified = true;
            await _context.SaveChangesAsync();

            return Ok("驗證成功");
        }

        // POST: api/PasswordResets/reset
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == dto.Email);
            if (member == null)
                return BadRequest("找不到該會員");

            var latestVerification = await _context.EmailVerificationCodes
                .Where(v => v.Email == dto.Email && v.VerificationType == EmailVerificationCode.VerificationTypeEnum.ResetPassword)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestVerification == null || !latestVerification.IsVerified)
                return BadRequest("尚未完成驗證碼驗證");

            // 密碼雜湊處理（根據你自己的加密邏輯）
            PasswordHasher.CreatePasswordHash(dto.NewPassword, out string hash, out string salt);
            member.PasswordHash = hash;
            member.PasswordSalt = salt;

            await _context.SaveChangesAsync();
            return Ok("密碼已重設成功");
        }

    }
}
