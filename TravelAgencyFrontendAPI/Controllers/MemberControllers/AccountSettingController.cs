using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.MemberDTOs;

namespace TravelAgencyFrontendAPI.Controllers.MemberControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountSettingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccountSettingController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/AccountSetting/profile?memberId=123
        [HttpGet("profile")]
        public async Task<ActionResult<AccountSettingDto>> GetProfile([FromQuery] int memberId)
        {
            try
            {
                var member = await _context.Members.FindAsync(memberId);
                if (member == null)
                    return NotFound("找不到會員資料");

                var dto = new AccountSettingDto
                {
                    MemberId = member.MemberId,
                    Name = member.Name,
                    Birthday = member.Birthday,
                    Email = member.Email,
                    Phone = member.Phone,
                    IdNumber = member.IdNumber,
                    Address = member.Address,
                    Nationality = member.Nationality,
                    Gender = member.Gender?.ToString(),
                    PassportSurname = member.PassportSurname,
                    PassportGivenName = member.PassportGivenName,
                    PassportExpireDate = member.PassportExpireDate,
                    DocumentType = member.DocumentType?.ToString(),
                    DocumentNumber = member.DocumentNumber,
                    ProfileImage = member.ProfileImage,
                    Note = member.Note,
                    UpdatedAt = member.UpdatedAt,
                    IsFakePhone = member.Phone != null && member.Phone.StartsWith("GPHONE"),
                };
                return Ok(dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❗ GetProfile 發生例外：" + ex.ToString());
                return StatusCode(500, "載入會員資料時發生錯誤，請稍後再試");
            }         
        }

        // PUT: api/AccountSetting/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] AccountSettingDto dto)
        {
            try
            {
                var member = await _context.Members.FindAsync(dto.MemberId);
                if (member == null)
                    return NotFound("找不到會員資料");

                // Email 不可更新
                member.Name = dto.Name;
                member.Birthday = dto.Birthday;
                member.Phone = dto.Phone;
                member.IdNumber = dto.IdNumber;
                member.Address = dto.Address;
                member.Nationality = dto.Nationality;

                if (Enum.TryParse<GenderType>(dto.Gender, out var gender))
                    member.Gender = gender;

                if (Enum.TryParse<DocumentType>(dto.DocumentType, out var docType))
                    member.DocumentType = docType;

                member.PassportSurname = dto.PassportSurname;
                member.PassportGivenName = dto.PassportGivenName;
                member.PassportExpireDate = dto.PassportExpireDate;
                member.DocumentNumber = dto.DocumentNumber;
                member.ProfileImage = dto.ProfileImage;
                member.Note = dto.Note;
                member.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❗ UpdateProfile 發生例外：" + ex.ToString());
                return StatusCode(500, "更新會員資料時發生錯誤，請稍後再試");
            }         
        }
    }
}
