using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DRES.Data;
using DRES.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DRES.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumptionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConsumptionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTOs

        public class CreateConsumptionDTO
        {
            [Required]
            [Range(1, int.MaxValue)]
            public int site_id { get; set; }

            [Required]
            [Range(1, int.MaxValue)]
            public int user_id { get; set; }

            [Required]
            public DateTime date { get; set; }

            public string? remark { get; set; }

            [Required]
            [MinLength(1, ErrorMessage = "At least one item is required.")]
            public List<CreateConsumptionItemDTO> items { get; set; } = new();
        }

        public class CreateConsumptionItemDTO
        {
            [Required]
            [Range(1, int.MaxValue)]
            public int material_id { get; set; }

            [Required]
            [Range(1, int.MaxValue)]
            public int unit_id { get; set; }

            [Required]
            [Range(1, int.MaxValue)]
            public int quantity { get; set; }
        }

        // POST: api/Material_Consumption/Create
        [HttpPost("AddConsumption")]
        public async Task<IActionResult> AddConsumption([FromBody] CreateConsumptionDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var site = await _context.Sites.FindAsync(dto.site_id);
            if (site == null)
                return NotFound(new { message = "Site not found." });

            var user = await _context.Users.FindAsync(dto.user_id);
            if (user == null)
                return NotFound(new { message = "User not found." });

           

            if (user.siteid != dto.site_id)
                return BadRequest(new { message = "User does not belong to the specified site." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var consumption = new Material_Consumption
                {
                    site_id = dto.site_id,
                    user_id = dto.user_id,
                    date = dto.date,
                    remark = dto.remark,
                    confirmed_by = null,
                    createdon = DateTime.Now
                };

                _context.UserAMaterial_ConsumptionctivityLogs.Add(consumption);
                await _context.SaveChangesAsync();

                foreach (var item in dto.items)
                {

                    var userexistingStock = await _context.Stocks
                       .FromSqlInterpolated(
                           $@"SELECT * FROM Stocks WITH (UPDLOCK) 
                               WHERE user_id = {dto.user_id} 
                                 AND material_id = {item.material_id} 
                                 AND unit_type_id = {item.unit_id} 
                                 AND StockOwnerType = 'engineer'")
                       .FirstOrDefaultAsync();

                    if (userexistingStock == null)
                    {
                        throw new Exception("Stock record not found for the specified criteria.");
                    }
                    if (userexistingStock.quantity < item.quantity)
                    {
                        throw new Exception(" stock not avaliable");
                    }
                    var consumptionItem = new Material_Consumption_Item
                    {
                        consumption_id = consumption.id,
                        material_id = item.material_id,
                        unit_id = item.unit_id,
                        quantity = item.quantity
                    };

                    userexistingStock.quantity -= item.quantity;
                    _context.Material_Consumption_Item.Add(consumptionItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Consumption record created successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

   
        // GET: api/Material_Consumption/GetConsumption/{userId}
        [HttpGet("GetConsumption/{userId}")]
        public async Task<IActionResult> GetConsumption(int userId)
        {
            try
            {
                // Get the user based on the provided userId.
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Prepare the query for Material Consumption including related Site, Items, Material, and Unit.
                IQueryable<Material_Consumption> query = _context.UserAMaterial_ConsumptionctivityLogs
                    .Include(c => c.Site)
                    .Include(c => c.Material_Consumption_Item)
                        .ThenInclude(i => i.Material)
                    .Include(c => c.Material_Consumption_Item)
                        .ThenInclude(i => i.Unit);

                // Filter the query based on the user's role.
                if (user.role == userrole.sitemanager)
                {
                    // For site managers, filter by the site id associated with the user.
                    query = query.Where(c => c.site_id == user.siteid);
                }
                else if (user.role == userrole.siteengineer)
                {
                    // For site engineers, filter consumption logs where the creating user is the current user.
                    query = query.Where(c => c.user_id == userId);
                }
                // For admin, no filter is applied.

                // Execute the query.
                var consumptionLogs = await query.OrderByDescending(c => c.id).ToListAsync();

                // Project the consumption logs into the response DTO.
                var response = consumptionLogs.Select(c => new
                {
                    c.id,
                    c.date,
                    site_name = c.Site != null ? c.Site.sitename : "N/A",
                    c.remark,
                    c.createdon,
                    items = c.Material_Consumption_Item.Select(i => new
                    {
                        i.id,
                        i.material_id,
                        material_name = i.Material?.material_name ?? "Unknown",
                        unit_name = i.Unit?.unitname ?? "Unknown",
                        unit_symbol = i.Unit?.unitsymbol ?? "",
                        i.quantity
                    }).ToList()
                }).ToList();

                return Ok(new { message = "Success", data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }



        [HttpGet("GetStock/")]
        public async Task<IActionResult> GetStock([FromQuery] int user_id, [FromQuery] int material_id, [FromQuery] int unit_type_id)
        {
            var stock = await _context.Stocks
                .FromSqlInterpolated(
                    $@"SELECT * FROM Stocks WITH (UPDLOCK)
                       WHERE user_id = {user_id} 
                         AND material_id = {material_id} 
                         AND unit_type_id = {unit_type_id} 
                         AND StockOwnerType = 'engineer'")
                .FirstOrDefaultAsync();

            // Return the quantity, or 0 if not found.
            int quantity = stock?.quantity ?? 0;
            return Ok(quantity);

        }

    }
}
