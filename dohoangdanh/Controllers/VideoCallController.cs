using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace dohoangdanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VideoCallController : ControllerBase
    {
        private readonly IConfiguration _cfg;
        public VideoCallController(IConfiguration cfg) { _cfg = cfg; }

        // GET api/VideoCall/token?room=ten-phong
        [HttpGet("token")]
        public IActionResult GetToken(string room)
        {
            var apiKey = _cfg["LiveKit:ApiKey"];
            var secret = _cfg["LiveKit:ApiSecret"];
            var wsUrl = _cfg["LiveKit:WsUrl"];

            // Kiểm tra cấu hình LiveKit đề phòng chưa thiết lập trong appsettings.json
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(wsUrl))
            {
                return BadRequest(new { message = "LiveKit configuration is missing in appsettings.json" });
            }

            var identity = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "user_anonymous";
            var name = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name) ?? identity;

            var grant = JsonSerializer.Serialize(new
            {
                room,
                roomJoin = true,
                canPublish = true,
                canSubscribe = true
            });

            var claims = new List<Claim>
            {
                new("sub", identity),
                new("name", name),
                new("video", grant, JsonClaimValueTypes.Json)
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: apiKey,
                claims: claims,
                notBefore: DateTime.UtcNow.AddMinutes(-1),
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(jwt),
                wsUrl,
                room
            });
        }
    }
}