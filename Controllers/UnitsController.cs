using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DRES.Data;
using DRES.Models;
using System.ComponentModel.DataAnnotations;

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

        // GET: api/Units
        [HttpGet("GetAllUnitTypes")]
        public async Task<IActionResult> GetAllUnitTypes()
        {
            var units = await _context.Units
                .OrderBy(u => u.unitname)
                .Select(u => new UnitDto
                {
                    Id = u.Id,
                    UnitName = u.unitname,
                    UnitSymbol = u.unitsymbol
                })
                .ToListAsync();

            if (!units.Any())
            {
                return NotFound(new
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "No unit types found"
                });
            }

            return Ok(units);
        }

       
        [HttpGet("GetUnitTypeById/{id}")]
        public async Task<IActionResult> GetUnitTypeById(int id)
        {
            var unit = await _context.Units
                .Where(u => u.Id == id)
                .Select(u => new UnitDto
                {
                    Id = u.Id,
                    UnitName = u.unitname,
                    UnitSymbol = u.unitsymbol
                })
                .FirstOrDefaultAsync();

            if (unit == null)
            {
                return NotFound(new
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = $"Unit type with ID {id} not found"
                });
            }

            return Ok(unit);
        }
        public class UnitDto
        {
            public int Id { get; set; }

            [Required]
            [StringLength(50)]
            public string UnitName { get; set; }

            [Required]
            [StringLength(10)]
            public string UnitSymbol { get; set; }
        }
        // PUT: api/Units/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUnit(int id, Unit unit)
        {
            if (id != unit.Id)
            {
                return BadRequest();
            }

            _context.Entry(unit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UnitExists(id))
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

        // POST: api/Units
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Unit>> PostUnit(Unit unit)
        {
            _context.Units.Add(unit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUnit", new { id = unit.Id }, unit);
        }

        // DELETE: api/Units/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null)
            {
                return NotFound();
            }

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UnitExists(int id)
        {
            return _context.Units.Any(e => e.Id == id);
        }


        
    }


}

