using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;
using System.Threading.Tasks;
using Azure;
using DRES.Data;
using DRES.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;

namespace DRES.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Material_RequestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public Material_RequestController(ApplicationDbContext context)
        {
            _context = context;
        }


        public class CreateMaterialRequestDTO
        {
            // Material_Request fields

            //[Required(ErrorMessage = "Site ID is required.")]
            //[Range(1, int.MaxValue, ErrorMessage = "Site ID must be greater than 0.")]
            //public int site_id { get; set; }

            [Required(ErrorMessage = "RequestedBy (User ID) is required.")]
            [Range(1, int.MaxValue, ErrorMessage = "RequestedBy must be greater than 0.")]
            public int requested_by { get; set; }

            public string? remark { get; set; }  // Optional

            [Required(ErrorMessage = "At least one request item is required.")]
            public List<CreateMaterialRequestItemDTO> items { get; set; } = new();
        }

        // DTO for each material request item
        public class CreateMaterialRequestItemDTO
        {
            [Required(ErrorMessage = "Material ID is required.")]
            [Range(1, int.MaxValue, ErrorMessage = "Material ID must be greater than 0.")]
            public int material_id { get; set; }

            [Required(ErrorMessage = "Unit ID is required.")]
            [Range(1, int.MaxValue, ErrorMessage = "Unit ID must be greater than 0.")]
            public int unit_id { get; set; }

            [Required(ErrorMessage = "Quantity is required.")]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
            public int quantity { get; set; }
        }


        // DTO for response on Material_Request
        public class MaterialRequestResponseDTO
        {
            public int id { get; set; }
            public string site_name { get; set; }
            public DateTime request_date { get; set; }
            public string requested_by { get; set; }
            public string? remark { get; set; }
            public string? approved_by { get; set; }
            public DateTime? approval_date { get; set; }
            public string status { get; set; }
            public List<MaterialRequestItemResponseDTO> items { get; set; } = new List<MaterialRequestItemResponseDTO>();
        }

        // DTO for response on each Material_Request_Item
        public class MaterialRequestItemResponseDTO
        {
            public int id { get; set; }
            public int request_id { get; set; }
            public string material_name { get; set; }
            public string unit_name { get; set; }
            public string unit_symbol { get; set; }
            public int quantity { get; set; }
            
        }

        // GET: api/Material_Request/GetAllRequests
        [HttpGet("GetRequestsList/{userId}")]
        public async Task<IActionResult> GetRequestsList(int userId)
        {
            try
            {
                // Get the user based on the provided userId.
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                IQueryable<Material_Request> query = _context.Material_Requests
                    .Include(r => r.Site)
                    .Include(r => r.Material_Request_Item)
                        .ThenInclude(i => i.Material)
                    .Include(r => r.Material_Request_Item)
                        .ThenInclude(i => i.Unit);

                if (user.role == userrole.sitemanager)
                {
                    query = query.Where(r => r.site_id == user.siteid && r.forwarded_to_ho == false);
                }
                else if (user.role == userrole.siteengineer)
                {
                    query = query.Where(r => r.requested_by == userId);
                }
                else if (user.role == userrole.admin)
                {
                    query = query.Where(r => r.forwarded_to_ho == true);
                }

                var requests = await query.OrderByDescending(r => r.id).ToListAsync();

                // Collect all unique user IDs for requested_by and approved_by.
                var userIds = requests.Select(r => r.requested_by)
                    .Union(requests.Where(r => r.approved_by != null).Select(r => r.approved_by.Value))
                    .Distinct()
                    .ToList();

                var users = await _context.Users
                    .Where(u => userIds.Contains(u.id))
                    .ToListAsync();

                var response = requests.Select(r => new MaterialRequestResponseDTO
                {
                    id = r.id,
                    site_name = r.Site != null ? r.Site.sitename : "N/A",
                    request_date = r.request_date,
                    // Lookup the requesting user and format as "username (role)".
                    requested_by = users.FirstOrDefault(u => u.id == r.requested_by) is User reqUser
                        ? $"{reqUser.username} ({reqUser.role})"
                        : "Unknown",
                    remark = r.remark,
                    approved_by = r.approved_by != null && users.FirstOrDefault(u => u.id == r.approved_by) is User apprUser
                        ? $"{apprUser.username} ({apprUser.role})"
                        : null,
                    approval_date = r.approval_date,
                    status = r.status,
                    items = r.Material_Request_Item.Select(i => new MaterialRequestItemResponseDTO
                    {
                        id = i.id,
                        request_id = i.request_id,
                        material_name = i.Material?.material_name ?? "Unknown",
                        unit_name = i.Unit?.unitname ?? "Unknown",
                        unit_symbol = i.Unit?.unitsymbol ?? "",
                        quantity = i.quantity,
                    }).ToList()
                }).ToList();

                return Ok(new { message = "Success", data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }






        // POST: api/Material_Request/CreateRequest
        [HttpPost("CreateRequest")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateMaterialRequestDTO requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }



            // Check if the user exists
            var user = await _context.Users.FindAsync(requestDto.requested_by);

            if (user == null)
            {
                return NotFound(new { message = "Invalid requested_by (User ID)." });
            }

            

            // Check if the site exists
            var site = await _context.Sites.FindAsync(user.siteid);
            if (site == null)
            {
                return NotFound(new { message = "Invalid site ID." });
            }
            bool forwardedToHo = (user.role == userrole.sitemanager);

            // Start a transaction to ensure atomicity.
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var newRequest = new Material_Request
                    {
                        site_id = site.id,
                        request_date = DateTime.Now,
                        requested_by = requestDto.requested_by,
                        remark = requestDto.remark,
                        approved_by = null,
                        approval_date = null,
                        status= "Pending",
                        forwarded_to_ho= forwardedToHo
                    };

                    _context.Material_Requests.Add(newRequest);
                    await _context.SaveChangesAsync();

                    foreach (var itemDto in requestDto.items)
                    {
                        var newItem = new Material_Request_Item
                        {
                            request_id = newRequest.id,
                            material_id = itemDto.material_id,
                            unit_id = itemDto.unit_id,
                            quantity = itemDto.quantity
                        };

                        _context.Material_Request_Item.Add(newItem);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { message = "Success" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { message = $"Error: {ex.Message}" });
                }
            }


        }
    }
}





