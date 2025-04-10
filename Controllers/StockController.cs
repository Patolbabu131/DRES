using System;
using System.Collections.Generic;
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
    public class StockController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StockController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTO for individual stock response
        public class StockDTO
        {
            public int? site_id { get; set; }
            public string site_name { get; set; }
            public string material_name { get; set; }
            public string unit_symbol { get; set; }
            public int quantity { get; set; }
        }

        // GET: api/Stock/GetAllStocks
        [HttpGet("GetAllStocks")]
        public async Task<IActionResult> GetAllStocks()
        {
            try
            {
                var stocks = await _context.Stocks
                    .Include(s => s.Material)
                    .Include(s => s.Site)
                    .Include(s => s.Unit)
                    .OrderByDescending(s => s.site_id)
                    .ThenBy(s => s.Material != null ? s.Material.id : int.MinValue)
                    .Select(s => new StockDTO
                    {
                        site_id = s.site_id,
                        site_name = s.site_id.HasValue
                            ? (s.Site != null ? s.Site.sitename : "Unknown Site")
                            : "Admin Warehouse",
                        material_name = s.Material != null
                            ? s.Material.material_name
                            : "Unknown Material",
                        unit_symbol = s.Unit != null
                            ? s.Unit.unitsymbol
                            : "",
                        quantity = s.quantity
                    })
                    .ToListAsync();

                return Ok(new { message = "Success", data = stocks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving stocks: {ex.Message}" });
            }
        }
    }
}