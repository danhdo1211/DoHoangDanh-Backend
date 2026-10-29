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
    public class DevicesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public DevicesController(ApplicationDbContext db) { _db = db; }

        int Uid => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        bool IsAdmin => User.IsInRole("Admin");

        // Parent xem thiết bị của các con mình, Admin xem tất cả
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var q = _db.Devices.AsQueryable();
            if (!IsAdmin)
            {
                var childIds = _db.Users.Where(u => u.ParentId == Uid).Select(u => u.Id);
                q = q.Where(d => childIds.Contains(d.ChildId));
            }
            return Ok(await q.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var d = await _db.Devices.FindAsync(id);
            return d == null ? NotFound() : Ok(d);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string name) =>
            Ok(await _db.Devices.Where(d => d.Name.Contains(name)).ToListAsync());

        [Authorize(Roles = "Admin,Parent")]
        [HttpPost]
        public async Task<IActionResult> Create(Device d)
        {
            d.Id = 0;
            _db.Devices.Add(d);
            await _db.SaveChangesAsync();
            return Ok(d);
        }

        [Authorize(Roles = "Admin,Parent")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Device d)
        {
            if (id != d.Id) return BadRequest();
            _db.Entry(d).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin,Parent")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var d = await _db.Devices.FindAsync(id);
            if (d == null) return NotFound();
            _db.Devices.Remove(d);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}