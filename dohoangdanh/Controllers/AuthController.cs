using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dohoangdanh.Models;
using dohoangdanh.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace dohoangdanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _cfg;
        private readonly EmailService _mail;

        public AuthController(ApplicationDbContext db, IConfiguration cfg, EmailService mail)
        { _db = db; _cfg = cfg; _mail = mail; }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email đã tồn tại");
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Role = dto.Role == "Admin" ? "Admin" : "User"
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return Ok("Đăng ký thành công");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Sai email hoặc mật khẩu");

            var otp = Random.Shared.Next(100000, 999999).ToString();
            user.Otp = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(5);
            await _db.SaveChangesAsync();

            try { await _mail.SendAsync(user.Email, "Mã OTP đăng nhập", $"Mã OTP của bạn: {otp} (hết hạn sau 5 phút)"); }
            catch { /* chưa cấu hình mail thì bỏ qua */ }

            if (_cfg.GetValue<bool>("ShowOtpInResponse"))
                return Ok(new { message = "Đã gửi OTP", otp });
            return Ok(new { message = "Đã gửi OTP về email" });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(OtpDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || user.Otp != dto.Otp || user.OtpExpiry < DateTime.UtcNow)
                return Unauthorized("OTP sai hoặc hết hạn");

            user.Otp = null;
            user.OtpExpiry = null;
            await _db.SaveChangesAsync();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                _cfg["Jwt:Issuer"], _cfg["Jwt:Audience"], claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                role = user.Role,
                userId = user.Id,
                fullName = user.FullName
            });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me() => Ok(new
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            email = User.FindFirstValue(ClaimTypes.Email),
            role = User.FindFirstValue(ClaimTypes.Role)
        });

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-users")]
        public async Task<IActionResult> AllUsers() =>
            Ok(await _db.Users.Select(u => new { u.Id, u.Email, u.FullName, u.Role }).ToListAsync());



        [Authorize(Roles = "User,Parent")]
        [HttpPost("create-child")]
        public async Task<IActionResult> CreateChild(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email đã tồn tại");
            var parentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var child = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Role = "Child",
                ParentId = parentId
            };
            _db.Users.Add(child);
            await _db.SaveChangesAsync();
            return Ok(new { child.Id, child.Email });
        }
    }
}