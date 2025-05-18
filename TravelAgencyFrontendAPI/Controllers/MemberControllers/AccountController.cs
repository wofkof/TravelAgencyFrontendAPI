//using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.DTOs.MemberDTOs;
using TravelAgencyFrontendAPI.Helpers;
using System.Text.RegularExpressions;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using Newtonsoft.Json.Linq;



namespace TravelAgencyFrontendAPI.Controllers.MemberControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public AccountController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // POST: api/Account/signup 
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto dto)
        {
            bool hasError = false;

            // 姓名格式驗證
            if (!IsValidName(dto.Name))
            {
                ModelState.AddModelError("Name", "姓名格式錯誤，僅能包含中英文，且不可含數字或特殊符號");
                hasError = true;
            }

            // 密碼驗證
            if (!IsValidPassword(dto.Password))
            {
                ModelState.AddModelError("Password", "密碼格式不正確（需設定長度6~12位數，且包含大、小寫英文的密碼）");
                hasError = true;
            }

            // Email 格式驗證 + 重複驗證
            if (!IsValidEmail(dto.Email))
            {
                ModelState.AddModelError("Email", "Email格式錯誤");
                hasError = true;
            }
            else if (await _context.Members.AnyAsync(m => m.Email == dto.Email))
            {
                ModelState.AddModelError("Email", "此信箱已被註冊");
                hasError = true;
            }

            // 手機格式驗證 + 重複驗證
            if (!IsValidPhone(dto.Phone))
            {
                ModelState.AddModelError("Phone", "手機號碼格式錯誤，需為09開頭的10碼數字");
                hasError = true;
            }
            else if (await _context.Members.AnyAsync(m => m.Phone == dto.Phone))
            {
                ModelState.AddModelError("Phone", "此手機號碼已被使用");
                hasError = true;
            }

            if (hasError)
            {
                return ValidationProblem(ModelState);
            }
            // 🔐 比對驗證碼
            var verification = await _context.EmailVerificationCodes
            .FirstOrDefaultAsync(e => e.Email == dto.Email &&
                                  e.VerificationType == EmailVerificationCode.VerificationTypeEnum.SignUp &&
                                  !e.IsVerified);

            if (verification == null)
            {
                ModelState.AddModelError("EmailVerificationCode", "請先發送驗證碼");
                return ValidationProblem(ModelState);
            }

            if (verification.VerificationCode != dto.EmailVerificationCode || verification.ExpireAt < DateTime.Now)
            {
                ModelState.AddModelError("EmailVerificationCode", "驗證碼錯誤或已過期");
                return ValidationProblem(ModelState);
            }

            // 標記驗證成功
            verification.IsVerified = true;

            // 密碼雜湊處理
            PasswordHasher.CreatePasswordHash(dto.Password, out string hash, out string salt);

            var member = new Member
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = hash,
                PasswordSalt = salt,
                RegisterDate = DateTime.Now,
                Status = MemberStatus.Active,
                IsEmailVerified = true

            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();          
            return Ok("註冊成功，確定後將跳轉回登入頁");
        }

        // ==== 驗證封裝區塊 ====
        private bool IsValidName(string name)
        {
            return Regex.IsMatch(name, @"^[\u4e00-\u9fa5a-zA-Z\s]{2,30}$");
        }

        private bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z]).{6,12}$");
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[\w.-]+@([\w-]+\.)+[a-zA-Z]{2,}$"
);
        }

        private bool IsValidPhone(string phone)
        {
            return Regex.IsMatch(phone, @"^09\d{8}$");
        }

        // POST: api/Account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 帳號比對已註冊的 Email 或 Phone 欄位
            var member = await _context.Members
                .SingleOrDefaultAsync(m => m.Email == dto.Account || m.Phone == dto.Account);

            if (member == null)
            {
                return Unauthorized("帳號或密碼錯誤");
            }

            bool isValid = PasswordHasher.VerifyPassword(dto.Password, member.PasswordHash, member.PasswordSalt);
            if (!isValid)
            {
                return Unauthorized("帳號或密碼錯誤");
            }

            return Ok(new
            {
                name = member.Name,
                id = member.MemberId
            });
        }

        // POST: api/Account/send-email-code
        [HttpPost("send-email-code")]
        public async Task<IActionResult> SendEmailVerificationCode([FromBody] SendVerificationCodeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email 為必填欄位");

            if (await _context.Members.AnyAsync(m => m.Email == dto.Email))
                return BadRequest("此 Email 已被註冊");

            var code = new Random().Next(100000, 999999).ToString();

            // 新增或更新驗證碼
            var existing = await _context.EmailVerificationCodes
                .FirstOrDefaultAsync(e => e.Email == dto.Email && e.VerificationType == EmailVerificationCode.VerificationTypeEnum.SignUp
);

            if (existing != null)
            {
                existing.VerificationCode = code;
                existing.CreatedAt = DateTime.Now;
                existing.ExpireAt = DateTime.Now.AddMinutes(10);
                existing.IsVerified = false;
            }
            else
            {
                _context.EmailVerificationCodes.Add(new EmailVerificationCode
                {
                    Email = dto.Email,
                    VerificationCode = code,
                    VerificationType = EmailVerificationCode.VerificationTypeEnum.SignUp,
                    CreatedAt = DateTime.Now,
                    ExpireAt = DateTime.Now.AddMinutes(10),
                    IsVerified = false
                });
            }

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                dto.Email,
                "歡迎註冊會員 - 驗證碼通知",
                $@"
                <div style='font-family:Arial,sans-serif; font-size:16px; color:#333; line-height:1.8'>
                  <div style='text-align:center; margin-bottom:20px'>
                    <img src='https://i.postimg.cc/kgC50Qfb/logo.png' alt='嶼你同行 LOGO' width='180' />
                  </div>

                  <p>親愛的旅客您好，</p>
                  <p>感謝您註冊 <strong>嶼你同行</strong>，以下是您的 Email 驗證碼：</p>

                  <div style='text-align:center; margin:20px 0'>
                    <span style='font-size:28px; font-weight:bold; color:#1d4ed8'>{code}</span>
                  </div>

                  <p>請於 <strong>10 分鐘</strong> 內完成註冊流程。</p>

                  <hr style='margin:30px 0; border:none; border-top:1px solid #ddd' />

                 <p style='font-size:14px; color:#888'>
                  若您並未申請註冊，請忽略此信件。<br>
                  此為系統自動發送的通知信件，請勿直接回覆。
                </p>
                  <p>嶼你同行 客服中心 敬上</p>
                </div>
                "
            );

            return Ok("驗證碼已寄出");
        }
    }
}
