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
    public class UnitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UnitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTOs
        public class CreateUnitRequest
        {
            [Required]
            public string unitname { get; set; }

            [Required]
            public string unitsymbol { get; set; }
        }

        public class UpdateUnitRequest
        {
            [Required]
            public int Id { get; set; }

            [Required]
            public string unitname { get; set; }

            [Required]
            public string unitsymbol { get; set; }
        }

        public class UnitResponse
        {
            public int Id { get; set; }
            public string unitname { get; set; }
            public string unitsymbol { get; set; }
        }

        // POST: api/Units/CreateUnit
        [HttpPost("CreateUnit")]
        public async Task<IActionResult> CreateUnit([FromBody] CreateUnitRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request data" });

                // Check for duplicate unique fields
                if (await _context.Units.AnyAsync(u => u.unitname == request.unitname))
                    return Conflict(new { message = "Unit name already exists. Please use a different name." });

                if (await _context.Units.AnyAsync(u => u.unitsymbol == request.unitsymbol))
                    return Conflict(new { message = "Unit symbol already exists. Please use a different symbol." });

                var newUnit = new Unit
                {
                    unitname = request.unitname,
                    unitsymbol = request.unitsymbol
                };

                _context.Units.Add(newUnit);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Unit created successfully", data = MapToResponse(newUnit) });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error creating unit: {ex.Message}" });
            }
        }

        // GET: api/Units/GetAllUnits
        [HttpGet("GetAllUnits")]
        public async Task<IActionResult> GetAllUnits()
        {
            try
            {
                var units = await _context.Units
                     .OrderByDescending(r => r.Id)
                    .Select(u => MapToResponse(u))
                    .ToListAsync();

                return Ok(new { data = units });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error retrieving units: {ex.Message}" });
            }
        }

        // GET: api/Units/GetUnitById/{id}
        [HttpGet("GetUnitById/{id}")]
        public async Task<IActionResult> GetUnitById(int id)
        {
            try
            {
                var unit = await _context.Units.FindAsync(id);
                if (unit == null)
                    return NotFound(new { message = "Unit not found" });

                return Ok(new { data = MapToResponse(unit) });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error retrieving unit: {ex.Message}" });
            }
        }

        // PUT: api/Units/UpdateUnit/{id}
        [HttpPut("UpdateUnit/{id}")]
        public async Task<IActionResult> UpdateUnit(int id, [FromBody] UpdateUnitRequest request)
        {
            try
            {
                if (!ModelState.IsValid || id != request.Id)
                    return BadRequest(new { message = "Invalid request data" });

                var existingUnit = await _context.Units.FindAsync(id);
                if (existingUnit == null)
                    return NotFound(new { message = "Unit not found" });

                // Check for duplicate unit name (excluding current unit)
                if (await _context.Units.AnyAsync(u => u.Id != id && u.unitname == request.unitname))
                    return Conflict(new { message = "Unit name already exists. Please use a different name." });

                // Check for duplicate unit symbol (excluding current unit)
                if (await _context.Units.AnyAsync(u => u.Id != id && u.unitsymbol == request.unitsymbol))
                    return Conflict(new { message = "Unit symbol already exists. Please use a different symbol." });

                existingUnit.unitname = request.unitname;
                existingUnit.unitsymbol = request.unitsymbol;

                _context.Entry(existingUnit).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Unit updated successfully", data = MapToResponse(existingUnit) });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error updating unit: {ex.Message}" });
            }
        }

        // DELETE: api/Units/DeleteUnit/{id}
        [HttpDelete("DeleteUnit/{id}")]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            try
            {
                var unit = await _context.Units.FindAsync(id);
                if (unit == null)
                    return NotFound(new { message = "Unit not found" });

                _context.Units.Remove(unit);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    // Check if the exception is due to a foreign key constraint violation
                    if (dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 547)
                    {
                        return BadRequest(new { message = "This unit is currently in use and cannot be deleted." });
                    }
                    return StatusCode((int)HttpStatusCode.InternalServerError,
                        new { message = $"Error deleting unit: {dbEx.Message}" });
                }
                catch (Exception ex)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError,
                        new { message = $"Error deleting unit: {ex.Message}" });
                }

                return Ok(new { message = "Unit deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"Error processing delete request: {ex.Message}" });
            }
        }

        private static UnitResponse MapToResponse(Unit unit) =>
            new UnitResponse
            {
                Id = unit.Id,
                unitname = unit.unitname,
                unitsymbol = unit.unitsymbol
            };
    }
}
