using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DRES.Data;
using DRES.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DRES.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly SymmetricSecurityKey _securityKey;
        private readonly string _issuer;
        private readonly string _audience;

        public AuthController(
           IConfiguration configuration,
           ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;

            // Initialize token parameters once in constructor
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            _issuer = _configuration["Jwt:Issuer"];
            _audience = _configuration["Jwt:Audience"];
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            if (login == null || string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest(new { message = "Invalid login data" });
            }

            try
            {
                // Use AsNoTracking for read-only operations
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.username == login.Username);

                if (user == null)
                    return Unauthorized(new { message = "Invalid UserName" });

                if (user.passwordhash != login.Password)
                    return Unauthorized(new { message = "Invalid Password" });

                // Generate JWT with role
                var tokenString = GenerateJwtToken(user);

                // Create and add user activity log
                var userActivityLog = new UserActivityLog
                {
                    userid = user.id,
                    ipaddress = login.IpAddress,
                    devicetype = login.DeviceType,
                    location = login.Location,
                    createdat = GetIndianTime(),
                    jwttoken = tokenString
                };

                // Add without tracking changes to the existing entity
                _context.UserActivityLogs.Add(userActivityLog);
                await _context.SaveChangesAsync();

                return Ok(new { token = tokenString });
            }
            catch (Exception)
            {
                // Log the exception elsewhere if needed
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

            // Pre-allocate array for better performance
            var claims = new Claim[4]
            {
                new Claim(ClaimTypes.Name, user.username),
                new Claim(ClaimTypes.Role, user.role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(3000),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private static DateTime GetIndianTime() =>
           TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
           TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string IpAddress { get; set; }
        public string DeviceType { get; set; }
        public string Location { get; set; }
    }
}