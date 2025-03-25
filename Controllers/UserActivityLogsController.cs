using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DRES.Data;
using DRES.Models;

namespace DRES.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserActivityLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserActivityLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UserActivityLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserActivityLog>>> GetUserActivityLogs()
        {
            return await _context.UserActivityLogs
                         .OrderByDescending(log => log.id)
                         .ToListAsync();
        }

        // GET: api/UserActivityLogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserActivityLog>> GetUserActivityLog(int id)
        {
            var userActivityLog = await _context.UserActivityLogs.FindAsync(id);

            if (userActivityLog == null)
            {
                return NotFound();
            }

            return userActivityLog;
        }

        // PUT: api/UserActivityLogs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserActivityLog(int id, UserActivityLog userActivityLog)
        {
            if (id != userActivityLog.id)
            {
                return BadRequest();
            }

            _context.Entry(userActivityLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserActivityLogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/UserActivityLogs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserActivityLog>> PostUserActivityLog(UserActivityLog userActivityLog)
        {
            _context.UserActivityLogs.Add(userActivityLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserActivityLog", new { id = userActivityLog.id }, userActivityLog);
        }

        // DELETE: api/UserActivityLogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserActivityLog(int id)
        {
            var userActivityLog = await _context.UserActivityLogs.FindAsync(id);
            if (userActivityLog == null)
            {
                return NotFound();
            }

            _context.UserActivityLogs.Remove(userActivityLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserActivityLogExists(int id)
        {
            return _context.UserActivityLogs.Any(e => e.id == id);
        }
    }
}
