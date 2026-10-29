using dohoangdanh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dohoangdanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppRulesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public AppRulesController(ApplicationDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetByDevice(int deviceId) =>
            Ok(await _db.AppRules.Where(r => r.DeviceId == deviceId).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var r = await _db.AppRules.FindAsync(id);
            return r == null ? NotFound() : Ok(r);
        }

        [Authorize(Roles = "Admin,Parent")]
        [HttpPost]
        public async Task<IActionResult> Create(AppRule r)
        {
            r.Id = 0;
            _db.AppRules.Add(r);
            await _db.SaveChangesAsync();
            return Ok(r);
        }

        [Authorize(Roles = "Admin,Parent")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AppRule r)
        {
            if (id != r.Id) return BadRequest();
            _db.Entry(r).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin,Parent")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _db.AppRules.FindAsync(id);
            if (r == null) return NotFound();
            _db.AppRules.Remove(r);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}