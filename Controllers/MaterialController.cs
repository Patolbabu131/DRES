using DRES.Data;
using DRES.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DRES.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaterialController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllMaterials")]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetAllMaterials()
        {
            return await _context.Materials
                .Include(m => m.Unit)  // Fixed the casing from 'unit' to 'Unit'
                .Select(m => new MaterialDto
                {
                    Id = m.id,
                    MaterialName = m.material_name,
                    UnitId = m.unit_id,
                    UnitName = m.Unit.unitname,
                    UnitSymbol = m.Unit.unitsymbol,
                    Remark = m.remark
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaterialDetailDto>> GetMaterial(int id)
        {
            var material = await _context.Materials
                .Include(m => m.Unit)
                .FirstOrDefaultAsync(m => m.id == id);

            if (material == null)
            {
                return NotFound();
            }

            return new MaterialDetailDto
            {
                Id = material.id,
                MaterialName = material.material_name,
                UnitId = material.unit_id,
                Remark = material.remark,
                Unit = new UnitDto
                {
                    Id = material.Unit.Id,
                    UnitName = material.Unit.unitname,
                    UnitSymbol = material.Unit.unitsymbol
                }
            };
        }
        // Add to MaterialController class
        [HttpPost("CreateMaterial")]
        public async Task<ActionResult<MaterialDetailDto>> CreateMaterial(CreateMaterialDto dto)
        {
            // Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if unit exists
            var unitExists = await _context.Units.AnyAsync(u => u.Id == dto.UnitId);
            if (!unitExists)
            {
                return BadRequest("Invalid Unit ID");
            }

            // Create new material
            var material = new Material
            {
                material_name = dto.MaterialName,
                unit_id = dto.UnitId,
                remark = dto.Remark
            };

            try
            {
                _context.Materials.Add(material);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Handle duplicate material name
                if (ex.InnerException is SqlException sqlEx &&
                    (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    return Conflict("Material name already exists");
                }
                return StatusCode(500, "An error occurred while saving");
            }

            // Return created material with full details
            var newMaterial = await _context.Materials
                .Include(m => m.Unit)
                .FirstOrDefaultAsync(m => m.id == material.id);

            return CreatedAtAction(nameof(GetMaterial), new { id = material.id },
                new MaterialDetailDto
                {
                    Id = newMaterial.id,
                    MaterialName = newMaterial.material_name,
                    UnitId = newMaterial.unit_id,
                    Remark = newMaterial.remark,
                    Unit = new UnitDto
                    {
                        Id = newMaterial.Unit.Id,
                        UnitName = newMaterial.Unit.unitname,
                        UnitSymbol = newMaterial.Unit.unitsymbol
                    }
                });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var material = await _context.Materials
                .Include(m => m.Stocks)
                .Include(m => m.Transactions)
                .FirstOrDefaultAsync(m => m.id == id);

            if (material == null)
            {
                return NotFound();
            }

            // Remove related stocks
            _context.Stocks.RemoveRange(material.Stocks);

            // Remove related transactions
            _context.Transactions.RemoveRange(material.Transactions);

            // Remove the material
            _context.Materials.Remove(material);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while deleting the material");
            }

            return NoContent();
        }

        // Add to DTO classes section
        public class CreateMaterialDto
        {
            [Required]
            [StringLength(100)]
            public string MaterialName { get; set; }

            [Required]
            public int UnitId { get; set; }

            [StringLength(500)]
            public string Remark { get; set; }
        }
    }


    public class MaterialDto
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string UnitSymbol { get; set; }
        public string Remark { get; set; }
    }

    public class MaterialDetailDto : MaterialDto
    {
        public UnitDto Unit { get; set; }
    }

    public class UnitDto
    {
        public int Id { get; set; }
        public string UnitName { get; set; }
        public string UnitSymbol { get; set; }
    }
}
