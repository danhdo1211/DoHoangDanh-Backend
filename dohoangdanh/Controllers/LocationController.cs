using System.Security.Claims;
using dohoangdanh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dohoangdanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public LocationController(ApplicationDbContext db) { _db = db; }

        public class LocDto
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        int MyId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Bé gửi vị trí (DeviceId lưu = Id tài khoản của bé)
        [Authorize(Roles = "Child")]
        [HttpPost]
        public async Task<IActionResult> Send(LocDto dto)
        {
            _db.LocationLogs.Add(new LocationLog
            {
                DeviceId = MyId,
                Lat = dto.Lat,
                Lng = dto.Lng
            });
            await _db.SaveChangesAsync();
            return Ok("OK");
        }

        // Phụ huynh xem vị trí mới nhất của các bé
        [Authorize(Roles = "User,Parent")]
        [HttpGet("children")]
        public async Task<IActionResult> Children()
        {
            var pid = MyId;
            var kids = await _db.Users
                .Where(u => u.ParentId == pid)
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();

            var result = new List<object>();
            foreach (var k in kids)
            {
                var last = await _db.LocationLogs
                    .Where(l => l.DeviceId == k.Id)
                    .OrderByDescending(l => l.CreatedAt)
                    .FirstOrDefaultAsync();
                result.Add(new
                {
                    k.Id,
                    k.FullName,
                    lat = last?.Lat,
                    lng = last?.Lng,
                    time = last?.CreatedAt
                });
            }
            return Ok(result);
        }
    }
}