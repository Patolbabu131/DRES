using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DRES.Data;
using DRES.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DRES.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTO for creating transaction
        public class CreateTransactionDTO
        {
            [Required(ErrorMessage = "Invoice number is required")]
            public string invoice_number { get; set; }

            [Required(ErrorMessage = "Transaction date is required")]
            public DateTime transaction_date { get; set; }

            public int? request_id { get; set; }

            public string? remark { get; set; }

            [Required(ErrorMessage = "Supplier or site origin is required")]
            public int? form_supplier_id { get; set; }

            [Required(ErrorMessage = "Destination site is required")]
            public int to_site_id { get; set; }

            [Required(ErrorMessage = "Created by user is required")]
            public int createdby { get; set; }

            [Required(ErrorMessage = "Transaction type is required")]
            public string transaction_type { get; set; }

            [Required(ErrorMessage = "At least one transaction item is required")]
            public List<CreateTransactionItemDTO> items { get; set; } = new();
        }

        // DTO for transaction items
        public class CreateTransactionItemDTO
        {
            [Required(ErrorMessage = "Material ID is required")]
            public int material_id { get; set; }

            [Required(ErrorMessage = "Unit type ID is required")]
            public int unit_type_id { get; set; }

            [Required(ErrorMessage = "Quantity is required")]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int quantity { get; set; }

            [Required(ErrorMessage = "Unit price is required")]
            [Range(0.00, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
            public decimal unit_price { get; set; }

            [Required(ErrorMessage = "GST percentage is required")]
            [Range(0, 18, ErrorMessage = "GST must be between 0 and 100")]
            public int gst { get; set; }

            [Required(ErrorMessage = "Total amount is required")]
            [Range(0.00, double.MaxValue, ErrorMessage = "Total must be greater than 0")]
            public decimal total { get; set; }
        }

        // DTO for transaction response
        public class TransactionResponseDTO
        {
            public int id { get; set; }
            public string invoice_number { get; set; }
            public DateTime transaction_date { get; set; }
            public string transaction_type { get; set; }
            public string? supplier_name { get; set; }
            public string from_site { get; set; }
            public string to_site { get; set; }
            public string created_by { get; set; }
            public string? remark { get; set; }
            public List<TransactionItemResponseDTO> items { get; set; } = new();
        }

        // DTO for transaction item response
        public class TransactionItemResponseDTO
        {
            public int id { get; set; }
            public string material_name { get; set; }
            public string unit_symbol { get; set; }
            public int quantity { get; set; }
            public decimal unit_price { get; set; }
            public int gst { get; set; }
            public decimal total { get; set; }
        }

        // POST: api/Transaction/Create
        [HttpPost("CreateTransaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDTO transactionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Validate related entities
                    var createdByUser = await _context.Users.FindAsync(transactionDto.createdby);
                    if (createdByUser == null)
                    {
                        return NotFound(new { message = "User not found" });
                    }

                    var toSite = await _context.Sites.FindAsync(transactionDto.to_site_id);
                    if (toSite == null)
                    {
                        return NotFound(new { message = "Destination site not found" });
                    }

                    // Create main transaction
                    var newTransaction = new Transaction
                    {
                        invoice_number = transactionDto.invoice_number,
                        transaction_date = transactionDto.transaction_date,
                        transaction_type = transactionDto.transaction_type,
                  
                        request_id = transactionDto.request_id,
                        remark = transactionDto.remark,
                        form_supplier_id = transactionDto.form_supplier_id,
                        to_site_id = transactionDto.to_site_id,
                        createdby = transactionDto.createdby,
                        createdat = DateTime.Now,
                        updatedby = transactionDto.createdby,
                        updatedat = DateTime.Now,
                        status = "completed" // Default status
                    };

                    _context.Transactions.Add(newTransaction);
                    await _context.SaveChangesAsync();

                    // Add transaction items
                    foreach (var itemDto in transactionDto.items)
                    {
                        var material = await _context.Materials.FindAsync(itemDto.material_id);
                        if (material == null)
                        {
                            throw new Exception($"Material with ID {itemDto.material_id} not found");
                        }

                        var unit = await _context.Units.FindAsync(itemDto.unit_type_id);
                        if (unit == null)
                        {
                            throw new Exception($"Unit with ID {itemDto.unit_type_id} not found");
                        }

                        var transactionItem = new Transaction_Items
                        {
                            transaction_id = newTransaction.id,
                            material_id = itemDto.material_id,
                            unit_type_id = itemDto.unit_type_id,
                            quantity = itemDto.quantity,
                            unit_price = itemDto.unit_price,
                            gst = itemDto.gst,
                            total = itemDto.total
                        };

                        _context.Transaction_Items.Add(transactionItem);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { message = "Transaction created successfully", transactionId = newTransaction.id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { message = $"Error creating transaction: {ex.Message}" });
                }
            }
        }

        // GET: api/Transaction/GetAll
        [HttpGet("GetAllTransactions")]
        public async Task<IActionResult> GetAllTransactions()
        {
            try
            {
                var transactions = await _context.Transactions
                    .Include(t => t.Supplier)
                    .Include(t => t.Site)
                    .Include(t => t.user)
                    .Include(t => t.TransactionItems)
                        .ThenInclude(i => i.Material)
                    .Include(t => t.TransactionItems)
                        .ThenInclude(i => i.Unit)
                    .OrderByDescending(t => t.transaction_date)
                    .ToListAsync();

                var response = transactions.Select(t => new TransactionResponseDTO
                {
                    id = t.id,
                    invoice_number = t.invoice_number,
                    transaction_date = t.transaction_date,
                    transaction_type = t.transaction_type,
                    supplier_name = t.Supplier?.company_name,
                    from_site = t.Site?.sitename ?? "Supplier",
                    to_site = _context.Sites.FirstOrDefault(s => s.id == t.to_site_id)?.sitename ?? "Unknown",
                    created_by = $"{t.user?.username} ({t.user?.role})",
                    remark = t.remark,
                    items = t.TransactionItems.Select(i => new TransactionItemResponseDTO
                    {
                        id = i.id,
                        material_name = i.Material?.material_name ?? "Unknown",
                        unit_symbol = i.Unit?.unitsymbol ?? "",
                        quantity = i.quantity,
                        unit_price = i.unit_price ?? 0,
                        gst = i.gst ?? 0,
                        total = i.total ?? 0
                    }).ToList()
                });

                return Ok(new { message = "Success", data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving transactions: {ex.Message}" });
            }
        }
    }
}