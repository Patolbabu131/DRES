

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
    public class SuppliersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTOs for Supplier
        public class CreateSupplierRequest
        {
            [Required]
            public string company_name { get; set; }


            public string contact_name { get; set; }

            [Required]
            public string gst { get; set; }

            public string phone1 { get; set; }
            public string address { get; set; }
        }

        public class UpdateSupplierRequest
        {
            [Required]
            public int id { get; set; }

            [Required]
            public string company_name { get; set; }

            [Required]
            public string contact_name { get; set; }

            [Required]
            public string gst { get; set; }

            public string phone1 { get; set; }
            public string address { get; set; }
        }

        public class SupplierResponse
        {
            public int id { get; set; }
            public string company_name { get; set; }
            public string contact_name { get; set; }
            public string gst { get; set; }
            public string phone1 { get; set; }
            public string address { get; set; }
            public string status { get; set; }
        }

        // Create
        [HttpPost("CreateSupplier")]
        public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Conflict checking for unique GST field
            if (await _context.Suppliers.AnyAsync(s => s.gst == request.gst))
            {
                return Conflict(new { message = "Duplicate GST entry" });
            }

            var supplier = new Supplier
            {
                company_name = request.company_name,
                contact_name = request.contact_name,
                gst = request.gst,
                phone1 = request.phone1,
                phone2 = "",
                address = request.address,
                // Set audit fields: createdat = GetIndianTime(), createdbyid = ...
            };

            _context.Suppliers.Add(supplier);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }

            return Ok(new { message = "Success", data = supplier });
        }

        // Get All
        [HttpGet("GetAllSuppliers")]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _context.Suppliers.OrderByDescending(r => r.id).ToListAsync();
            return Ok(new { message = "Success", data = suppliers });
        }

        // Get by Id
        [HttpGet("GetSupplierById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound(new { message = "Supplier not found" });
            }
            return Ok(new { message = "Success", data = supplier });
        }

        // Update
        [HttpPut("UpdateSupplier/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierRequest request)
        {
            if (id != request.id)
                return BadRequest(new { message = "ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound(new { message = "Supplier not found" });

            // Check for conflict if GST is changed and another supplier already has it
            if (supplier.gst != request.gst &&
                await _context.Suppliers.AnyAsync(s => s.gst == request.gst && s.id != id))
            {
                return Conflict(new { message = "Duplicate GST entry" });
            }

            supplier.company_name = request.company_name;
            supplier.contact_name = request.contact_name;
            supplier.gst = request.gst;
            supplier.phone1 = request.phone1;
            supplier.phone2 = "";
            supplier.address = request.address;
            // Update audit fields: updatedat = GetIndianTime(), updatedbyid = ...

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }

            return Ok(new { message = "Success", data = supplier });
        }

        // Delete
        [HttpDelete("DeleteSupplier/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound(new { message = "Supplier not found" });
            }

            _context.Suppliers.Remove(supplier);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Unable to delete supplier due to existing related transactions." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }

            return Ok(new { message = "Success" });
        }
    }
}
