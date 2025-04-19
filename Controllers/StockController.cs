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

        // DTO for individual material stock
        public class MaterialStockDTO
        {
            public string MaterialName { get; set; }
            public string UnitName { get; set; }
            public int SiteStock { get; set; }
            public IDictionary<string, int> EngineerStocks { get; set; }
        }

        // DTO for site grouping
        public class SiteStockDTO
        {
            public string SiteName { get; set; }
            public List<MaterialStockDTO> Stocks { get; set; }
        }

        // GET: api/Stock/GetAllStocks/{userId}
        [HttpGet("GetAllStocks/{userId}")]
        public async Task<IActionResult> GetAllStocks(int userId)
        {
            try
            {
                // 1. Fetch user and validate
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // 2. Base query for stocks
                var stocksQuery = _context.Stocks
                    .Include(s => s.Site)
                    .Include(s => s.Material)
                    .Include(s => s.Unit)
                    .Include(s => s.User)
                        .ThenInclude(u => u.Site)
                          .OrderByDescending(s => s.last_transaction_id)
                    .AsQueryable();

                // 3. Apply role-based filtering
                if (user.role == userrole.sitemanager)
                {
                    // Only stocks for this manager's site (site entries + engineers at this site)
                    stocksQuery = stocksQuery.Where(s =>
                        (s.StockOwnerType == "site" && s.Site.id == user.siteid) ||
                        (s.StockOwnerType == "engineer" && s.User.Site.id == user.siteid)
                    );
                }
                else if (user.role == userrole.siteengineer)
                {
                    // Only this engineer's personal stock
                    stocksQuery = stocksQuery.Where(s =>
                        s.StockOwnerType == "engineer" && s.User.id == userId
                    );
                }
                // Admin sees everything (no filter)

                // 4. Execute query
                var allStocks = await stocksQuery.ToListAsync();

                // 5. Flat grouping per site + material
                var flat = allStocks
                    .Where(s => s.StockOwnerType == "site" || s.StockOwnerType == "engineer")
                    .GroupBy(s => new {
                        SiteId = s.StockOwnerType == "site" ? s.Site.id : s.User.Site.id,
                        SiteName = s.StockOwnerType == "site" ? s.Site.sitename : s.User.Site.sitename,
                        MaterialName = s.Material.material_name,
                        UnitName = s.Unit.unitname
                    })
                    .Select(g => new {
                        g.Key.SiteId,
                        g.Key.SiteName,
                        g.Key.MaterialName,
                        g.Key.UnitName,
                        SiteStock = g.Where(x => x.StockOwnerType == "site").Sum(x => x.quantity),
                        EngineerStocks = g
                            .Where(x => x.StockOwnerType == "engineer")
                            .GroupBy(x => x.User.username)
                            .ToDictionary(eg => eg.Key, eg => eg.Sum(x => x.quantity))
                    })
                    .ToList();

                // 6. Pivot into nested per-site structure
                var result = flat
              .GroupBy(x => new { x.SiteId, x.SiteName })
              .Select(siteGroup => new SiteStockDTO
              {
                  SiteName = siteGroup.Key.SiteName,
                  Stocks = siteGroup.Select(item => new MaterialStockDTO
                  {
                      MaterialName = item.MaterialName,
                      UnitName = item.UnitName,
                      SiteStock = item.SiteStock,
                      EngineerStocks = item.EngineerStocks
                  }).ToList()
              })
              .ToList();

                // 7. Return structured response
                return Ok(new { message = "Success", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }
    }
}
