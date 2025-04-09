using DRES.Data;
using DRES.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DRES.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTO for creating Material
        public class CreateMaterialRequest
        {
            [Required]
            public string material_name { get; set; }

            public string remark { get; set; }
        }

        // Response DTO for Material
        public class MaterialResponse
        {
            public int id { get; set; }
            public string material_name { get; set; }
            public string remark { get; set; }
        }

        // Mapping function to convert Material entity to MaterialResponse DTO.
        private static MaterialResponse MapToResponse(Material material) =>
            new MaterialResponse
            {
                id = material.id,
                material_name = material.material_name,
                remark = material.remark
            };

        // Create Material
        [HttpPost("CreateMaterial")]
        public async Task<IActionResult> Create([FromBody] CreateMaterialRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if a material with the same name already exists (case-insensitive)
            var existingMaterial = await _context.Materials
                .FirstOrDefaultAsync(m => m.material_name.ToLower() == request.material_name.ToLower());

            if (existingMaterial != null)
            {
                return BadRequest(new { message = "Duplicate Material Name" });
            }

            try
            {
                var material = new Material
                {
                    material_name = request.material_name,
                    remark = request.remark
                };

                _context.Materials.Add(material);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Success", data = MapToResponse(material) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // Get All Materials
        [HttpGet("GetAllMaterials")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var materials = await _context.Materials.ToListAsync();
                var response = materials.OrderByDescending(r => r.id).Select(m => MapToResponse(m));
                return Ok(new { message = "Success", data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving materials: {ex.Message}" });
            }
        }

        // Get Material by Id
        [HttpGet("GetMaterialById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var material = await _context.Materials.FindAsync(id);
                if (material == null)
                {
                    return NotFound(new { message = "Material not found" });
                }

                return Ok(new { message = "Success", data = MapToResponse(material) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving material: {ex.Message}" });
            }
        }

        // Delete Material
        [HttpDelete("DeleteMaterial/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var material = await _context.Materials.FindAsync(id);
                if (material == null)
                {
                    return NotFound(new { message = "Material not found" });
                }

                _context.Materials.Remove(material);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return Conflict(new { message = "Unable to delete material due to existing related transactions." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = $"Error: {ex.Message}" });
                }

                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error processing delete request: {ex.Message}" });
            }
        }
    }
}
