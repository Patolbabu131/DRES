using DRES.Data;
using DRES.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace DRES.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SitesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SitesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTOs
        public class CreateSiteRequest
        {
            [Required] public string Sitename { get; set; }
            [Required] public string Siteaddress { get; set; }
            [Required] public string State { get; set; }
            [Required] public int Createdbyid { get; set; }
            public string Description { get; set; }
        }

        public class UpdateSiteRequest
        {
            [Required] public int Id { get; set; }
            [Required] public string Sitename { get; set; }
            [Required] public string Siteaddress { get; set; }
            [Required] public string State { get; set; }
            [Required] public int Updatedbyid { get; set; }
            public string Description { get; set; }
        }

        public class SiteResponse
        {
            public int Id { get; set; }
            public string Sitename { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public string Siteaddress { get; set; }
            public string State { get; set; }
        }

        // POST: api/sites
        [HttpPost("CreateSite")]
        public async Task<IActionResult> CreateSite([FromBody] CreateSiteRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request data" });

                // Check for existing site name
                if (await _context.Sites.AnyAsync(s => s.sitename == request.Sitename))
                    return Conflict(new { message = "Site name already exists. Please use a different name." });

                var newSite = new Site
                {
                    sitename = request.Sitename,
                    siteaddress = request.Siteaddress,
                    state = request.State,
                    description = request.Description,
                    status = SiteStatus.Active,
                    createdbyid = request.Createdbyid,
                    updatedbyid = request.Createdbyid,
                    createdat = GetIndianTime(),
                    updatedat = GetIndianTime()
                };

                _context.Sites.Add(newSite);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Site Created Successfully", data = MapToResponse(newSite) });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error creating site: {ex.Message}" });
            }
        }

        // GET: api/sites
        [HttpGet("GetAllSites")]
        public async Task<IActionResult> GetAllSites()
        {
            try
            {
                var sites = await _context.Sites
                    .Where(s => s.id != 1) // Exclude site with id = 1
                    .OrderByDescending(r => r.id).Select(s => MapToResponse(s))


                    .ToListAsync();

                return Ok(new { data = sites });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error retrieving sites: {ex.Message}" });
            }
        }

        // GET: api/sites/5
        [HttpGet("GetSiteById/{id}")]
        public async Task<IActionResult> GetSiteById(int id)
        {
            try
            {
                if (id == 1) // Explicitly block access to site with id = 1
                    return NotFound(new { message = "Site not found" });

                var site = await _context.Sites.FindAsync(id);

                if (site == null)
                    return NotFound(new { message = "Site not found" });

                return Ok(new { data = MapToResponse(site) });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error retrieving site: {ex.Message}" });
            }
        
        }

        // PUT: api/sites/5
        [HttpPut("UpdateSite/{id}")]
        public async Task<IActionResult> UpdateSite(int id, [FromBody] UpdateSiteRequest request)
        {
            try
            {
                if (id == 1) // Block deletion of the site with id = 1
                    return BadRequest(new { message = "Edit of this site is not allowed." });

                if (!ModelState.IsValid || id != request.Id)
                    return BadRequest(new { message = "Site ID not found, Invalid request data" });

                var existingSite = await _context.Sites.FindAsync(id);
                if (existingSite == null)
                    return NotFound(new { message = "Site not found" });

                // Check for duplicate site name (excluding current site)
                if (await _context.Sites.AnyAsync(s => s.id != id && s.sitename == request.Sitename))
                    return Conflict(new { message = "Site name already exists. Please use a different name." });

                existingSite.sitename = request.Sitename;
                existingSite.siteaddress = request.Siteaddress;
                existingSite.state = request.State;
                existingSite.description = request.Description;
                existingSite.updatedbyid = request.Updatedbyid;
                existingSite.updatedat = GetIndianTime();

                _context.Entry(existingSite).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Site Updated Successfully", data = MapToResponse(existingSite) });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error Updating Site: {ex.Message}" });
            }


        }

        [HttpDelete("DeleteSite/{id}")]
        public async Task<IActionResult> DeleteSite(int id)
        {
            try
            {
                if (id == 1) // Block deletion of the site with id = 1
                    return BadRequest(new { message = "Deletion of this site is not allowed." });

                var site = await _context.Sites.FindAsync(id);
                if (site == null)
                    return NotFound(new { message = "Site not found" });

                _context.Sites.Remove(site);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Site Deleted Successfully" });
            }
            catch (DbUpdateException dbEx)
            {
                // Check if the inner exception is a SQL exception indicating a foreign key constraint violation
                if (dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 547)
                {
                    return BadRequest(new { message = "This Site is currently in use and cannot be deleted." });
                }

                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error deleting site: {dbEx.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error deleting site: {ex.Message}" });
            }
        }



        private static SiteResponse MapToResponse(Site site) => new SiteResponse
        {
            Id = site.id,
            Sitename = site.sitename,
            Description = site.description,
            Status = site.status.ToString(),
            Siteaddress = site.siteaddress,
            State = site.state
        };

        private static DateTime GetIndianTime() =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
    }
}