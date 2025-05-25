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
        public async Task<IActionResult> RequestReset([FromBody] EmailDto dto)
        {           
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email 欄位是空的，請正確傳送 Email 資料");

            string email = dto.Email.Trim().ToLower();

            try
            {
                // 檢查會員是否存在
                var member = await _context.Members.FirstOrDefaultAsync(m => m.Email.ToLower() == email);
                if (member == null)
                    return BadRequest("查無此會員信箱，請確認輸入正確");

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

                // 發送信件
                try
                {
                    await _emailService.SendEmailAsync(
                        email,
                        "嶼你同行｜密碼重設驗證碼通知",
                        $@"<div style='font-family:Arial,sans-serif; font-size:16px; color:#333; line-height:1.8'>
                  <div style='text-align:center; margin-bottom:20px'>
                    <img src='https://i.postimg.cc/kgC50Qfb/logo.png' alt='嶼你同行 LOGO' width='180' />
                  </div>
                  <p>親愛的旅客您好，</p>
                  <p>我們收到您提出的 <strong>密碼重設申請</strong>，以下是您的 Email 驗證碼：</p>
                  <div style='text-align:center; margin:20px 0'>
                    <span style='font-size:28px; font-weight:bold; color:#1d4ed8'>{code}</span>
                  </div>
                  <p>請於 <strong>10 分鐘</strong> 內完成驗證並設定新密碼。</p>
                  <hr style='margin:30px 0; border:none; border-top:1px solid #ddd' />
                  <p style='font-size:14px; color:#888'>
                    若您並未申請密碼重設，請忽略此信件。<br>
                    此為系統自動發送的通知信件，請勿直接回覆。
                  </p>
                  <p>嶼你同行 客服中心 敬上</p>
              </div>"
                    );
                }
                catch (Exception sendEx)
                {
                    // 加入獨立 SMTP 錯誤處理，方便追蹤信件問題
                    var smtpError = $"❌ 寄信失敗：{sendEx.GetType().Name}: {sendEx.Message}\n{sendEx.StackTrace}";
                    Console.WriteLine(smtpError);
                    return StatusCode(500, smtpError); // 回傳寄信錯誤
                }

                return Ok("✅ 驗證碼已成功寄出至信箱");
            }
            catch (Exception ex)
            {
                var generalError = $"❌ 系統錯誤：{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
                Console.WriteLine(generalError);
                return StatusCode(500, generalError); // 回傳系統錯誤
            }
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
            member.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok("密碼已重設成功");
        }

    }
}
