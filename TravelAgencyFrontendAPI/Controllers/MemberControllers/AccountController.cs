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
            //var existing = await _context.Members.FirstOrDefaultAsync(m => m.Email == dto.Email);

            //if (existing == null)
            //{
            //    ModelState.AddModelError("EmailVerificationCode", "請先發送驗證碼");
            //    return ValidationProblem(ModelState);
            //}

            //if (existing.EmailVerificationCode != dto.EmailVerificationCode ||
            //    existing.EmailVerificationExpireTime < DateTime.Now)
            //{
            //    ModelState.AddModelError("EmailVerificationCode", "驗證碼錯誤或已過期，請重新輸入");
            //    return ValidationProblem(ModelState);
            //}

            // 產生 6 碼驗證碼
            var code = new Random().Next(100000, 999999).ToString();


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

                // ✅ 寄送驗證碼用欄位
                EmailVerificationCode = code,
                EmailVerificationExpireTime = DateTime.Now.AddMinutes(10),
                IsEmailVerified = false
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            // ✅ 寄出 Email 驗證碼
            await _emailService.SendEmailAsync(
                member.Email,
                "嶼你同行 - 註冊驗證碼",
                $"您好，歡迎加入嶼你同行！<br><br>您的驗證碼為：<b>{code}</b><br><br>請於 10 分鐘內完成信箱驗證，以啟用您的帳戶。"
            );
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
            {
                return BadRequest("Email 為必填欄位");
            }

            // ❗ 改為禁止重複註冊的判斷（如果該 Email 已存在就不再發送）
            if (await _context.Members.AnyAsync(m => m.Email == dto.Email))
            {
                return BadRequest("此 Email 已被註冊，請直接登入或使用其他信箱");
            }

            // 然後繼續產生驗證碼、寄出 Email（可存入暫存區或前端自己保存）
            var code = new Random().Next(100000, 999999).ToString();

            // 可選：將驗證碼儲存在伺服器記憶體/快取/資料表（這段未寫，可日後擴充）

            try
            {
                await _emailService.SendEmailAsync(
                    dto.Email,
                    "嶼你同行 - 註冊驗證碼",
                    $"您好，這是您的 Email 驗證碼：<b>{code}</b><br>請在 10 分鐘內完成驗證。"
                );

                return Ok("驗證碼已發送");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"寄信錯誤：{ex.Message}");
                return StatusCode(500, $"驗證碼寄送失敗：{ex.Message}");
            }

        }

        // POST: api/Account/verify-email-code
        [HttpPost("verify-email-code")]
        public async Task<IActionResult> VerifyEmailCode([FromBody] VerifyEmailCodeDto dto)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == dto.Email);

            if (member == null)
                return NotFound("找不到該會員");

            if (member.IsEmailVerified)
                return BadRequest("該信箱已驗證過");

            if (member.EmailVerificationExpireTime < DateTime.Now)
                return BadRequest("驗證碼已過期，請重新取得");

            if (member.EmailVerificationCode != dto.Code)
                return BadRequest("驗證碼錯誤");

            // 驗證成功
            member.IsEmailVerified = true;
            member.EmailVerificationCode = null;
            member.EmailVerificationExpireTime = null;

            await _context.SaveChangesAsync();

            return Ok("信箱驗證成功");
        }

    }
}
