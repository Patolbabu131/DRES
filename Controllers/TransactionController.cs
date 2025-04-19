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
            public decimal? grand_total { get; set; } // Added
            public List<TransactionItemResponseDTO> items { get; set; } = new();
        }

        public class CreateIssueTransactionDTO
        {
            
            [Required(ErrorMessage = "Site ID is required")]
            public int site_id { get; set; }
            public int? request_id { get; set; }

            public string? remark { get; set; }
            [Required(ErrorMessage = "From site ID is required")]
            public int from_site_id { get; set; }
            [Required(ErrorMessage = "To user ID is required")]
            public int to_user_id { get; set; }

            // Created by field comes from request
            [Required(ErrorMessage = "Created by user is required")]
            public int createdby { get; set; }

            // At least one transaction item is required
            [Required(ErrorMessage = "At least one transaction item is required")]
            public List<CreateIssueTransactionItemDTO> items { get; set; } = new();
        }
        public class CreateIssueTransactionItemDTO
        {
            [Required(ErrorMessage = "Material ID is required")]
            public int material_id { get; set; }

            [Required(ErrorMessage = "Unit type ID is required")]
            public int unit_type_id { get; set; }

            [Required(ErrorMessage = "Quantity is required")]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int quantity { get; set; }
        }



        public class TransactionItemResponseDTO
        {
            public int id { get; set; }
            public string material_name { get; set; }
            public string unit_symbol { get; set; }
            public int quantity { get; set; }
            public decimal unit_price { get; set; }
            public int gst { get; set; }
            public decimal? texable { get; set; } // Added
            public decimal total { get; set; }
        }
            
        [HttpPost("CreateSiteTransaction")]
        public async Task<IActionResult> CreateSiteTransaction([FromBody] CreateTransactionDTO transactionDto)
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
                        site_id=transactionDto.to_site_id,
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

                    // Variable to accumulate grand_total
                    decimal grandTotal = 0;

                    // Add transaction items and update stock
                    foreach (var itemDto in transactionDto.items.OrderBy(i => i.material_id))
                    {
                        // Validate material and unit existence
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

                        // Calculate texable and validate total
                        decimal texable = itemDto.quantity * itemDto.unit_price;
                        decimal taxRate = (decimal)itemDto.gst / 100m;
                        decimal expectedTotal = texable * (1 + taxRate);
                        expectedTotal = Math.Round(expectedTotal, 2); // Round to 2 decimal places

                        // Validate provided total matches calculation (with tolerance for floating-point precision)
                        if (Math.Abs(expectedTotal - itemDto.total) > 0.01m)
                        {
                            throw new Exception($"Provided total ({itemDto.total}) does not match calculated total ({expectedTotal}) for material ID {itemDto.material_id}");
                        }

                        // Create the transaction item record
                        var transactionItem = new Transaction_Items
                        {
                            transaction_id = newTransaction.id,
                            material_id = itemDto.material_id,
                            unit_type_id = itemDto.unit_type_id,
                            quantity = itemDto.quantity,
                            unit_price = itemDto.unit_price,
                            gst = itemDto.gst,
                            texable = texable, // Set calculated texable
                            total = itemDto.total // Use provided total after validation
                        };

                        _context.Transaction_Items.Add(transactionItem);

                        // Accumulate grand_total
                        grandTotal += itemDto.total;

                        // Update stock logic
                      
                        var existingStock = await _context.Stocks
                                .FromSqlInterpolated(
                                    $@"SELECT * FROM Stocks WITH (UPDLOCK) 
                                       WHERE site_id = {transactionDto.to_site_id} 
                                         AND material_id = {itemDto.material_id} 
                                         AND unit_type_id = {itemDto.unit_type_id} 
                                         AND StockOwnerType = 'site'")
                                .FirstOrDefaultAsync();
                        if (existingStock == null)
                        {
                            var newStock = new Stock
                            {
                                material_id = itemDto.material_id,
                                site_id = transactionDto.to_site_id,
                                unit_type_id = itemDto.unit_type_id,
                                last_transaction_id = newTransaction.id,
                                StockOwnerType="site",
                                quantity = itemDto.quantity,
                                UpdatedAt = DateTime.Now
                            };
                            _context.Stocks.Add(newStock);
                        }
                        else
                        {
                            existingStock.quantity += itemDto.quantity;
                            existingStock.UpdatedAt = DateTime.Now;
                            existingStock.last_transaction_id = newTransaction.id;
                        }
                    }

                    // Set grand_total for the transaction
                    newTransaction.grand_total = grandTotal;

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


        [HttpPost("IssueMaterialTransaction")]
        public async Task<IActionResult> IssueMaterialTransaction([FromBody] CreateIssueTransactionDTO transactionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Start a database transaction scope to ensure data consistency
            using (var dbTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                   

                    // Validate createdby user exists
                    var createdByUser = await _context.Users.FindAsync(transactionDto.to_user_id);
                    if (createdByUser == null)
                    {
                        return NotFound(new { message = "User ID not found" });
                    }
                    if (createdByUser.siteid != transactionDto.site_id)
                    {
                        return NotFound(new { message = "User not belong to this site" });
                    }
                    if (createdByUser.role != userrole.siteengineer)
                    {
                        return NotFound(new { message = "invalid user" });
                    }

                    
                    // Validate from_site exists
                    var fromSite = await _context.Sites.FindAsync(transactionDto.from_site_id);
                    if (fromSite == null)
                    {
                        return NotFound(new { message = "From site not found" });
                    }

                    // Validate that to user (site engineer) exists if required (optional validation)
                    var toUser = await _context.Users.FindAsync(transactionDto.to_user_id);
                    if (toUser == null)
                    {
                        return NotFound(new { message = "Destination user (site engineer) not found" });
                    }

                    // Create the issue transaction record
                    var newTransaction = new Transaction
                    {
                        invoice_number = null,
                        transaction_type = "issue",
                        transaction_date = DateTime.Now,
                        site_id = transactionDto.site_id,
                        request_id = transactionDto.request_id,
                        remark = transactionDto.remark,
                        form_supplier_id = null,
                        from_site_id = transactionDto.from_site_id,
                        to_site_id=null,
                        to_user_id = transactionDto.to_user_id,
                        status = "pending",
                        received_by_user_id = null,
                        recived_datetime = null,
                        grand_total = null,
                        createdby = transactionDto.createdby,
                        createdat = DateTime.Now,
                        updatedby = transactionDto.createdby,
                        updatedat = DateTime.Now
                    };

                    _context.Transactions.Add(newTransaction);
                    await _context.SaveChangesAsync();

                    // Process each transaction item
                    foreach (var itemDto in transactionDto.items.OrderBy(i => i.material_id))
                    {

                        var siteStock = await _context.Stocks
                            .FromSqlInterpolated(
                                $@"SELECT * FROM Stocks WITH (UPDLOCK) 
                                   WHERE site_id = {transactionDto.site_id} 
                                     AND material_id = {itemDto.material_id} 
                                     AND unit_type_id = {itemDto.unit_type_id} 
                                     AND StockOwnerType = 'site'")
                            .FirstOrDefaultAsync();

                        if (siteStock == null)
                        {
                            throw new Exception("Stock record not found for the specified criteria.");
                        }
                        if (siteStock.quantity < itemDto.quantity)
                        {
                            throw new Exception($"Insufficient stock. Available: {siteStock.quantity}, Requested: {itemDto.quantity}");
                        }

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

                        // Create the transaction item record; pricing, gst, tax-related fields are left as null
                        var transactionItem = new Transaction_Items
                        {
                            transaction_id = newTransaction.id, // assigned after saving the transaction
                            material_id = itemDto.material_id,
                            unit_type_id = itemDto.unit_type_id,
                            quantity = itemDto.quantity,
                            unit_price = null,
                            tex_type = null,
                            gst = null,
                            texable = null,
                            total = null
                        };

                        _context.Transaction_Items.Add(transactionItem);

                        var existingStock = await _context.Stocks
                        .FromSqlInterpolated(
                            $@"SELECT * FROM Stocks WITH (UPDLOCK) 
                               WHERE user_id = {transactionDto.to_user_id} 
                                 AND material_id = {itemDto.material_id} 
                                 AND unit_type_id = {itemDto.unit_type_id} 
                                 AND StockOwnerType = 'engineer'")
                        .FirstOrDefaultAsync();

                        // Subtract stock from the issuing site exactly once per transaction item.
                        siteStock.quantity -= itemDto.quantity;

                        if (existingStock == null)
                        {
                            var newStock = new Stock
                            {
                                material_id = itemDto.material_id,
                                user_id = transactionDto.to_user_id,
                                unit_type_id = itemDto.unit_type_id,
                                last_transaction_id = newTransaction.id,
                                StockOwnerType = "engineer",
                                quantity = itemDto.quantity,
                                UpdatedAt = DateTime.Now
                            };
                            _context.Stocks.Add(newStock);
                        }
                        else
                        {
                            existingStock.quantity += itemDto.quantity;
                            existingStock.UpdatedAt = DateTime.Now;
                            existingStock.last_transaction_id = newTransaction.id;
                        }

                    }


                    await _context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    return Ok(new { message = "Issue transaction created successfully", transactionId = newTransaction.id });
                }
                catch (Exception ex)
                {
                    await dbTransaction.RollbackAsync();
                    return StatusCode(500, new { message = $"Error creating issue transaction: {ex.Message}" });
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
                    .OrderByDescending(t => t.id)
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
                    remark = t.remark,
                    grand_total = t.grand_total, // Added
                    items = t.TransactionItems.Select(i => new TransactionItemResponseDTO
                    {
                        id = i.id,
                        material_name = i.Material?.material_name ?? "Unknown",
                        unit_symbol = i.Unit?.unitsymbol ?? "",
                        quantity = i.quantity,
                        unit_price = i.unit_price ?? 0,
                        gst = i.gst ?? 0,
                        texable = i.texable, // Added
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






        [HttpGet("GetSiteStock/")]
        public async Task<IActionResult> GetSiteStock([FromQuery] int site_id, [FromQuery] int material_id, [FromQuery] int unit_type_id)
        {
            var siteStock = await _context.Stocks
                            .FromSqlInterpolated(
                                $@"SELECT * FROM Stocks WITH (UPDLOCK) 
                                   WHERE site_id = {site_id} 
                                     AND material_id = {material_id} 
                                     AND unit_type_id = {unit_type_id} 
                                     AND StockOwnerType = 'site'")
                            .FirstOrDefaultAsync();

            // Return the quantity, or 0 if not found.
            int quantity = siteStock?.quantity ?? 0;
            return Ok(quantity);

        }


    }
}