//using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs;
using TravelAgencyFrontendAPI.DTOs.MemberDTOs;
using TravelAgencyFrontendAPI.Models;
using TravelAgencyFrontendAPI.Helpers;
using System.Text.RegularExpressions;



namespace TravelAgencyFrontendAPI.Controllers.MemberControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Account/signup 
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto dto)
        {
            bool hasError = false;

            // 密碼驗證
            if (!IsValidPassword(dto.Password))
            {
                ModelState.AddModelError("Password", "密碼格式不正確（需包含大、小寫英文與特殊符號，長度6-12字元）");
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

            // 手機格式驗證
            if (!IsValidPhone(dto.Phone))
            {
                ModelState.AddModelError("Phone", "手機號碼格式錯誤，需為09開頭共10碼數字");
                hasError = true;
            }

            if (hasError)
            {
                return ValidationProblem(ModelState);
            }

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
                Status = MemberStatus.Active
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return Ok("註冊成功");
        }

        // ==== 驗證封裝區塊 ====
        private bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{6,12}$");
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[\w.-]+@[\w-]+\.[a-zA-Z]{2,}$");
        }

        private bool IsValidPhone(string phone)
        {
            return Regex.IsMatch(phone, @"^09\d{8}$");
        }



        // POST: api/Account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var member = await _context.Members.SingleOrDefaultAsync(m => m.Email == dto.Email);
            if (member == null)
            {
                return Unauthorized("帳號或密碼錯誤");
            }

            bool isValid = PasswordHasher.VerifyPassword(dto.Password, member.PasswordHash, member.PasswordSalt);
            if (!isValid)
            {
                return Unauthorized("帳號或密碼錯誤");
            }

            return Ok("登入成功");
        }
    }
}
