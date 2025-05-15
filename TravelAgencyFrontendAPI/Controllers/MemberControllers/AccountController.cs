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
        public AccountController(AppDbContext context)
        {
            _context = context;
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

    }
}
