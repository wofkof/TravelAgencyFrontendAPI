//using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.DTOs.MemberDTOs;
using TravelAgencyFrontendAPI.Helpers;
using System.Text.RegularExpressions;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text;



namespace TravelAgencyFrontendAPI.Controllers.MemberControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly string _recaptchaSecret;
        private readonly IConfiguration _config;
        private string GenerateFakePhone()
        {
            return "GPHONE" + Guid.NewGuid().ToString("N")[..8]; // 例如 GPHONEabc123ef
        }


        public AccountController(AppDbContext context, EmailService emailService, IConfiguration config)
        {
            _context = context;
            _emailService = emailService;
            _recaptchaSecret = config["GoogleReCaptcha:SecretKey"];
            _config = config;
        }

        // POST: api/Account/signup 
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto dto)
        {
            try
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
                    ModelState.AddModelError("EmailVerificationCode", "請先完成信箱驗證");
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
            catch (Exception ex)
            {
                Console.WriteLine("❗ 註冊發生例外：" + ex.ToString());
                return StatusCode(500, "註冊時發生錯誤，請稍後再試");
            }          
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
            try
            {
                // ✅ Step 1：驗證 Google reCAPTCHA token
                if (string.IsNullOrWhiteSpace(dto.RecaptchaToken))
                {
                    return BadRequest("請先完成機器人驗證");
                }

                // 發送驗證請求給 Google
                using var httpClient = new HttpClient();
                var secret = _recaptchaSecret;

                var response = await httpClient.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={dto.RecaptchaToken}",
                    null
                );

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<RecaptchaVerifyResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result is not { Success: true })
                {
                    return BadRequest("reCAPTCHA 驗證失敗");
                }
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
            catch (Exception ex)
            {
                Console.WriteLine("❗ 登入發生例外：" + ex.ToString());
                return StatusCode(500, "登入失敗，請稍後再試");
            }
            
        }

        // POST: api/Account/send-email-code
        [HttpPost("send-email-code")]
        public async Task<IActionResult> SendEmailVerificationCode([FromBody] SendVerificationCodeDto dto)
        {
            try
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
                    "嶼你同行｜歡迎註冊會員-驗證碼通知",
                    $@"
                <div style='font-family:Arial,sans-serif; font-size:16px; color:#333; line-height:1.8'>
                  <div style='text-align:center; margin-bottom:20px'>
                    <img src='https://i.ibb.co/bgLz9Hk3/logo.png' alt='嶼你同行 LOGO' width='180' />
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
            catch (Exception ex)
            {
                Console.WriteLine("❗ 寄送驗證碼發生例外：" + ex.ToString());
                return StatusCode(500, "寄送驗證碼時發生錯誤，請稍後再試");
            }          
        }

        // PUT: api/Account/change-password
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                    return BadRequest("請填寫完整欄位");

                if (dto.NewPassword != dto.ConfirmPassword)
                    return BadRequest("新密碼與確認密碼不一致");

                if (!IsValidPassword(dto.NewPassword))
                    return BadRequest("新密碼格式不正確（需包含大小寫英文字母，長度6~12位）");

                var member = await _context.Members.FindAsync(dto.MemberId);
                if (member == null)
                    return NotFound("找不到會員");

                if (!PasswordHasher.VerifyPassword(dto.OldPassword, member.PasswordHash, member.PasswordSalt))
                    return BadRequest("舊密碼錯誤");

                // 建立新密碼雜湊
                PasswordHasher.CreatePasswordHash(dto.NewPassword, out string newHash, out string newSalt);
                member.PasswordHash = newHash;
                member.PasswordSalt = newSalt;
                member.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok("密碼已成功更新");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❗ 修改密碼發生例外：" + ex.ToString());
                return StatusCode(500, "修改密碼時發生錯誤，請稍後再試");
            }       
        }

        
        // POST: api/Account/GoogleLogin
        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            Console.WriteLine("收到 Google code: " + dto.Code);

            try
            {
                var clientId = _config["GoogleOAuth:ClientId"];
                var clientSecret = _config["GoogleOAuth:ClientSecret"];
                var redirectUri = _config["GoogleOAuth:RedirectUri"];
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
                {
                    Console.WriteLine("❌ Google OAuth 設定缺失");
                    return StatusCode(500, "Google OAuth 設定尚未正確載入");
                }
                Console.WriteLine($"使用設定 - ClientId: {clientId}, RedirectUri: {redirectUri}");
                // 向 Google 換 token
                Console.WriteLine("準備向 Google 請求 token...");
                using var httpClient = new HttpClient();
                Console.WriteLine("建立請求內容...");
                
                var tokenResponse = await httpClient.PostAsync(
                    "https://oauth2.googleapis.com/token",                  
                new FormUrlEncodedContent(new Dictionary<string, string>
                    {
            { "code", dto.Code },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" }
                    })
                );
                Console.WriteLine("收到 Google 回應，開始讀取內容...");
                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                Console.WriteLine("Google 回傳狀態碼：" + tokenResponse.StatusCode);
                Console.WriteLine("Google 回傳內容：" + tokenJson);

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("❌ Google token 交換失敗");
                    return StatusCode(500, "❌ Google token 交換失敗：" + tokenJson);
                }

                var tokenDoc = JsonDocument.Parse(tokenJson);
                if (!tokenDoc.RootElement.TryGetProperty("id_token", out var idTokenElement))
                {
                    Console.WriteLine("❌ 回傳中找不到 id_token");
                    return StatusCode(500, "❌ 回傳中找不到 id_token，請確認 scope 是否包含 openid");
                }
                var idToken = idTokenElement.GetString();
                if (string.IsNullOrEmpty(idToken))
                {
                    Console.WriteLine("❌ id_token 為空");
                    return StatusCode(500, "❌ id_token 為空");
                }

                Console.WriteLine("開始解碼 id_token");
                var payload = DecodeIdToken(idToken);
                // 安全地取得使用者資訊
                if (!payload.TryGetValue("email", out var emailObj) || emailObj == null)
                {
                    Console.WriteLine("❌ 無法從 id_token 取得 email");
                    return StatusCode(500, "❌ 無法從 Google 取得使用者 email");
                }

                if (!payload.TryGetValue("name", out var nameObj) || nameObj == null)
                {
                    Console.WriteLine("❌ 無法從 id_token 取得 name");
                    return StatusCode(500, "❌ 無法從 Google 取得使用者姓名");
                }

                if (!payload.TryGetValue("sub", out var subObj) || subObj == null)
                {
                    Console.WriteLine("❌ 無法從 id_token 取得 sub");
                    return StatusCode(500, "❌ 無法從 Google 取得使用者識別碼");
                }
                var email = emailObj.ToString();
                var name = nameObj.ToString();
                var googleId = subObj.ToString();

                Console.WriteLine($"取得使用者資訊 - Email: {email}, Name: {name}, GoogleId: {googleId}");

                // 查找或建立會員（用 Email 為主）
                try
                {
                    var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == email);

                    if (member != null && !string.IsNullOrEmpty(member.GoogleId) && member.GoogleId != googleId)
                    {
                        Console.WriteLine("❌ Email 已被其他 Google 帳號使用");
                        return BadRequest("此 Email 已是本網站會員");
                    }

                    if (member == null)
                    {
                        Console.WriteLine("建立新會員");
                        member = new Member
                        {
                            Name = name,
                            Email = email,
                            GoogleId = googleId,
                            IsEmailVerified = true,
                            RegisterDate = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            Status = MemberStatus.Active,
                            PasswordHash = "-",
                            PasswordSalt = "-",
                            IsBlacklisted = false,
                            //電話是用假號碼，因為google登入並不一定會有電話值
                            Phone = GenerateFakePhone()
                        };
                        _context.Members.Add(member);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ 新會員建立成功，ID: {member.MemberId}");
                    }
                    else
                    {
                        // 如果會員存在但沒有 GoogleId，更新它
                        if (string.IsNullOrEmpty(member.GoogleId))
                        {
                            member.GoogleId = googleId;
                            member.UpdatedAt = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                            Console.WriteLine($"✅ 現有會員已綁定 Google 帳號，ID: {member.MemberId}");
                        }
                        else
                        {
                            Console.WriteLine($"✅ 現有 Google 會員登入，ID: {member.MemberId}");
                        }
                    }

                    return Ok(new
                    {
                        memberId = member.MemberId,
                        name = member.Name
                    });
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine("❗ 資料庫操作發生例外：");
                    Console.WriteLine(dbEx.ToString());
                    return StatusCode(500, "資料庫操作失敗，請稍後再試");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❗ GoogleLogin 發生例外：");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("❗ 例外類型：" + ex.GetType().Name);
                Console.WriteLine("❗ 例外訊息：" + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("❗ 內部例外：" + ex.InnerException.Message);
                }
                return StatusCode(500, "伺服器內部錯誤，請稍後再試");
            }
        }
        
        private Dictionary<string, object> DecodeIdToken(string idToken)
        {
            var parts = idToken.Split('.');
            if (parts.Length < 2)
                throw new Exception("id_token 格式錯誤");

            var payload = parts[1];
            var base64 = payload.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
                case 1: base64 += "==="; break;
            }

            try
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DecodeIdToken 發生錯誤：" + ex.Message);
                throw new Exception("❌ id_token 解碼失敗，內容格式錯誤", ex);
            }
        }
        }
}
