﻿//using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.DTOs;
using TravelAgencyFrontendAPI.DTOs.MemberDTOs;
using TravelAgencyFrontendAPI.Helpers;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TravelAgencyFrontendAPI.DTOs.FavoriteTravelerDTOs;
using TravelAgency.Shared.Models;
using TravelAgency.Shared.Data;



namespace TravelAgencyFrontendAPI.Controllers.FavoriteTravelerController
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteTravelerController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FavoriteTravelerController(AppDbContext context)
        {
            _context = context;
        }

        //取得某會員所有常用旅客
        [HttpGet("{memberId}")]
        public async Task<ActionResult<IEnumerable<FavoriteTravelerResponseDto>>> GetByMemberId(int memberId)
        {
            try 
            {
                var travelers = await _context.MemberFavoriteTravelers
                .Where(t => t.MemberId == memberId && t.Status == FavoriteStatus.Active)
                .Select(t => new FavoriteTravelerResponseDto
                {
                    FavoriteTravelerId = t.FavoriteTravelerId,
                    Name = t.Name,
                    Phone = t.Phone,
                    IdNumber = t.IdNumber,
                    BirthDate = t.BirthDate,
                    Gender = t.Gender,
                    Email = t.Email,
                    DocumentType = t.DocumentType,
                    DocumentNumber = t.DocumentNumber,
                    PassportSurname = t.PassportSurname,
                    PassportGivenName = t.PassportGivenName,
                    PassportExpireDate = t.PassportExpireDate,
                    Nationality = t.Nationality,
                    Note = t.Note
                }).ToListAsync();

                return Ok(travelers);
            }
            catch (Exception ex)
            {
                // 可用 Logger 記錄 ex.Message
                return StatusCode(500, "伺服器錯誤，請稍後再試");
            }           
        }

        //新增旅客
        [HttpPost]
        public async Task<IActionResult> CreateTraveler([FromBody] FavoriteTravelerDto dto)
        {
            try
            {
                var validationResult = ValidateTaiwanId(dto);
                if (validationResult != null)
                    return validationResult;
                var traveler = new MemberFavoriteTraveler
                {
                    MemberId = dto.MemberId,
                    Name = dto.Name,
                    Phone = dto.Phone,
                    IdNumber = dto.IdNumber,
                    BirthDate = dto.BirthDate,
                    Gender = dto.Gender,
                    Email = dto.Email,
                    DocumentType = dto.DocumentType,
                    DocumentNumber = dto.DocumentNumber,
                    PassportSurname = dto.PassportSurname,
                    PassportGivenName = dto.PassportGivenName,
                    PassportExpireDate = dto.PassportExpireDate,
                    Nationality = dto.Nationality,
                    Note = dto.Note,
                    CreatedAt = DateTime.UtcNow,
                    Status = FavoriteStatus.Active
                };

                _context.MemberFavoriteTravelers.Add(traveler);
                await _context.SaveChangesAsync();

                return Ok(new { traveler.FavoriteTravelerId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "新增旅客時發生錯誤，請稍後再試");
            }         
        }
        //更新旅客
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTraveler(int id, [FromBody] FavoriteTravelerDto dto)
        {
            try
            {
                var validationResult = ValidateTaiwanId(dto);
                if (validationResult != null)
                    return validationResult;

                var traveler = await _context.MemberFavoriteTravelers.FindAsync(id);
                if (traveler == null || traveler.Status == FavoriteStatus.Deleted)
                    return NotFound();

                traveler.Name = dto.Name;
                traveler.Phone = dto.Phone;
                traveler.IdNumber = dto.IdNumber;
                traveler.BirthDate = dto.BirthDate;
                traveler.Gender = dto.Gender;
                traveler.Email = dto.Email;
                traveler.DocumentType = dto.DocumentType;
                traveler.DocumentNumber = dto.DocumentNumber;
                traveler.PassportSurname = dto.PassportSurname;
                traveler.PassportGivenName = dto.PassportGivenName;
                traveler.PassportExpireDate = dto.PassportExpireDate;
                traveler.Nationality = dto.Nationality;
                traveler.Note = dto.Note;
                traveler.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "更新旅客資料時發生錯誤，請稍後再試");
            }           
        }
        //軟刪除旅客
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTraveler(int id)
        {
            try
            {
                var traveler = await _context.MemberFavoriteTravelers.FindAsync(id);
                if (traveler == null || traveler.Status == FavoriteStatus.Deleted)
                    return NotFound();

                traveler.Status = FavoriteStatus.Deleted;
                traveler.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "刪除旅客時發生錯誤，請稍後再試");
            }          
        }
        private IActionResult? ValidateTaiwanId(FavoriteTravelerDto dto)
        {
            if (dto.Nationality == "TW")
            {
                if (string.IsNullOrWhiteSpace(dto.IdNumber))
                    return BadRequest("台灣國籍的旅客必須填寫身分證字號");

                var idRegex = new Regex("^[A-Z][1289]\\d{8}$");
                if (!idRegex.IsMatch(dto.IdNumber))
                    return BadRequest("台灣身分證字號格式不正確");
            }

            return null;
        }

    }
}
    
