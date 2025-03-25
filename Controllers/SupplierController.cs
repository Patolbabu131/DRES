using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DRES.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using DRES.Data;

namespace DRES.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/supplier/GetAllSuppliers
        [HttpGet("GetAllSuppliers")]
        public async Task<IActionResult> GetAllSuppliers()
        {
            var suppliers = await _context.Suppliers
                .Select(s => new SupplierResponseDto
                {
                    Id = s.id,
                    CompanyName = s.company_name,
                    ContactName = s.contact_name,
                    GST = s.gst,
                    Phone1 = s.phone1,
                    Phone2 = s.phone2,
                    Address = s.address
                })
                .ToListAsync();

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Suppliers retrieved successfully", suppliers));
        }

        // GET: api/supplier/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            var supplier = await _context.Suppliers
                .Where(s => s.id == id)
                .Select(s => new SupplierResponseDto
                {
                    Id = s.id,
                    CompanyName = s.company_name,
                    ContactName = s.contact_name,
                    GST = s.gst,
                    Phone1 = s.phone1,
                    Phone2 = s.phone2,
                    Address = s.address
                })
                .FirstOrDefaultAsync();

            if (supplier == null)
            {
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Supplier not found"));
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Supplier retrieved successfully", supplier));
        }
    }

    public class SupplierResponseDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string GST { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Address { get; set; }
    }
}