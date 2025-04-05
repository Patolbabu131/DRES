using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DRES.Data;
using DRES.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using System.Security.Policy;

namespace DRES.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => !(u.id == 3 || u.role == userrole.admin))
                    .Select(u => new UserResponse
                    {
                        Id = u.id,
                        Username = u.username,
                        Phone = u.phone,
                        Role = u.role.ToString(),
                        SiteName = u.Site != null ? u.Site.sitename : null
                    })
                    .ToListAsync();

                return Ok(new { data = users });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error retrieving users: {ex.Message}" });
            }
        }



        [HttpGet("GetUserById/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                //// Explicitly block access to protected user
                //if (id == 3)
                //    return NotFound(new { message = "User not found" });

                var user = await _context.Users
                    .Where(u => u.id == id && u.role != 0)
                    .Select(u => new UserResponse
                    {
                        Id = u.id,
                        Username = u.username,
                        Phone = u.phone,
                        Role = u.role.ToString(),
                        SiteName = u.Site != null ? u.Site.sitename : null
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                    return NotFound(new { message = "User not found" });

                return Ok(new { data = user });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error retrieving user: {ex.Message}" });
            }
        }



        [HttpGet("GetUsersBySiteId/{siteId}")]
        public async Task<IActionResult> GetUsersBySiteId(int siteId)
        {
            try
            {
                // Verify site exists
                var siteExists = await _context.Sites.AnyAsync(s => s.id == siteId);
                if (!siteExists)
                    return NotFound(new { message = "Site not found" });

                var users = await _context.Users
                    .Where(u => u.siteid == siteId && !(u.id == 3 && u.role == userrole.admin))
                    .Select(u => new UserResponse
                    {
                        Id = u.id,
                        Username = u.username,
                        Phone = u.phone,
                        Role = u.role.ToString(),
                        SiteName = u.Site.sitename
                    })
                    .ToListAsync();

                return Ok(new { data = users });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error retrieving users: {ex.Message}" });
            }
        }



        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request data" });

                // Check username uniqueness
                if (await _context.Users.AnyAsync(u => u.username == request.Username))
                    return Conflict(new { message = "Username already exists" });

                // Get creator user
                var creatorUser = await _context.Users.FindAsync(request.CreatedById);
                if (creatorUser == null)
                    return BadRequest(new { message = "Invalid creator ID" });

                // Role validation
                if (request.Role == userrole.admin)
                    return BadRequest(new { message = "Admin creation not allowed" });

                if (creatorUser.role == userrole.sitemanager && request.Role != userrole.siteengineer)
                    return BadRequest(new { message = "Site managers can only create site manager" });

                if (creatorUser.role == userrole.admin &&
                    request.Role != userrole.sitemanager &&
                    request.Role != userrole.siteengineer)
                {
                    return BadRequest(new { message = "Admins can only create site managers/engineers" });
                }

                // Site validation
                var siteExists = await _context.Sites.AnyAsync(s => s.id == request.SiteId);
                if (!siteExists)
                    return BadRequest(new { message = "Invalid Site ID" });

                // Generate password hash

                string usernameNoSpaces = request.Username.Replace(" ", "");
                string firstFourUsername = usernameNoSpaces.Length >= 4 ? usernameNoSpaces.Substring(0, 4) : usernameNoSpaces;
                string lastFourPhone = request.Phone.Length >= 4 ? request.Phone.Substring(request.Phone.Length - 4) : request.Phone;
                var password = $"{firstFourUsername}@{lastFourPhone}";

                var newUser = new User
                {
                    username = request.Username,
                    phone = request.Phone,
                    role = request.Role,
                    siteid = request.SiteId,
                    passwordhash = password,
                    IsActive = true,
                    createdbyid = request.CreatedById,
                    updatedbyid = request.CreatedById,
                    CreatedAt = GetIndianTime(),
                    UpdatedAt = GetIndianTime()
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                await _context.Entry(newUser).Reference(u => u.Site).LoadAsync();
                return Ok(new
                {
                    message = "User created successfully",
                    data = MapUserToResponse(newUser)
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error creating user: {ex.Message}" });
            }
        }



        [HttpPut("EditUser/{id}")]
        public async Task<IActionResult> EditUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid || id != request.Id)
                    return BadRequest(new { message = "Invalid request data" });

                // Block editing of protected admin user
                if (id == 3)
                    return BadRequest(new { message = "This user cannot be modified" });

                var existingUser = await _context.Users
                    .Include(u => u.Site)
                    .FirstOrDefaultAsync(u => u.id == id);

                if (existingUser == null)
                    return NotFound(new { message = "User not found" });

                // Validate editor exists
                var editorUser = await _context.Users.FindAsync(request.UpdatedById);
                if (editorUser == null)
                    return BadRequest(new { message = "Invalid editor ID" });

              

                // Username uniqueness check
                if (await _context.Users.AnyAsync(u => u.id != id && u.username == request.Username))
                    return Conflict(new { message = "Username already exists" });




                // Update fields
                existingUser.username = request.Username;
                existingUser.phone = request.Phone;
                existingUser.updatedbyid = request.UpdatedById;
                existingUser.UpdatedAt = GetIndianTime();

                _context.Entry(existingUser).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "User updated successfully",
                    data = MapUserToResponse(existingUser)
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error updating user: {ex.Message}" });
            }
        }




        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                // Block deletion of protected admin user
                if (id == 3)
                    return BadRequest(new { message = "Deletion of this user is not allowed." });

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User Deleted Successfully" });
            }
            catch (DbUpdateException dbEx)
            {
                // Handle foreign key constraint violations
                if (dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 547)
                {
                    return BadRequest(new
                    {
                        message = "User cannot be deleted as they have associated records in the system."
                    });
                }

                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error deleting user: {dbEx.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error deleting user: {ex.Message}" });
            }
        }


        // UpdateUserRequest DTO




        private static DateTime GetIndianTime() =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));


        public class UpdateUserRequest
        {
            [Required] public int Id { get; set; }
            [Required] public string Username { get; set; }
            [Required] public string Phone { get; set; }
            [Required] public int UpdatedById { get; set; }
        }
        public class CreateUserRequest
        {
            [Required] public string Username { get; set; }
            [Required] public string Phone { get; set; }
            [Required] public userrole Role { get; set; }
            [Required] public int SiteId { get; set; }
            [Required] public int CreatedById { get; set; }
        }


        private static UserResponse MapUserToResponse(User user) => new UserResponse
        {
            Id = user.id,
            Username = user.username,
            Phone = user.phone,
            Role = user.role.ToString(),
            SiteName = user.Site?.sitename
        };

        // Updated UserResponse
        public class UserResponse
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Phone { get; set; }
            public string Role { get; set; }
            public string SiteName { get; set; }
        }


    }
  
}

