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
            [Range(0, 100, ErrorMessage = "GST must be between 0 and 100")]
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
                        transaction_type = "Inward",
                        request_id = transactionDto.request_id,
                        remark = transactionDto.remark,
                        site_id = transactionDto.to_site_id,
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
                                StockOwnerType = "site",
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
                        transaction_type = "Issue To Engineer",
                        transaction_date = DateTime.Now,
                        site_id = transactionDto.site_id,
                        request_id = transactionDto.request_id,
                        remark = transactionDto.remark,
                        form_supplier_id = null,
                        from_site_id = transactionDto.from_site_id,
                        to_site_id = null,
                        to_user_id = transactionDto.to_user_id,
                        status = "completed",
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
                    if (transactionDto.request_id != null)
                    {
                        var request = await _context.Material_Requests.FindAsync(transactionDto.request_id);
                        if (request == null)
                        {
                            throw new Exception("Request not found");
                        }

                        

                            request.status = "Fulfilled";
                        request.approved_by = transactionDto.createdby;
                        request.approval_date = DateTime.Now;

                        await _context.SaveChangesAsync();
                    }
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


        public class CombinedTransactionDto
        {
            public string VoucherNo { get; set; }        // e.g. “P123” or “C456”
            public string SiteName { get; set; }         // from Site.Name
            public DateTime Date { get; set; }           // consumption.date or transaction.transaction_date
            public string TransactionType { get; set; }  // e.g. “Purchase”, “Consumption”
            public DateTime CreatedOn { get; set; }      // consumption.createdon or transaction.createdat
        }


        // GET: api/Transactions/GetAllTransactions/{userId}
        [HttpGet("GetAllTransactions/{userId}")]
        public async Task<IActionResult> GetAllTransactions(int userId)
        {
            try
            {
                // 1) Fetch the user
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                // 2) Start from base sets
                IQueryable<Material_Consumption> consBase = _context.UserAMaterial_ConsumptionctivityLogs;
                IQueryable<Transaction> txBase = _context.Transactions;

                // 3) Apply per-role filters
                switch (user.role)
                {
                    case userrole.admin:
                        // no filtering
                        break;

                    case userrole.sitemanager:
                        consBase = consBase.Where(mc => mc.site_id == user.siteid);
                        txBase = txBase.Where(t => t.site_id == user.siteid);
                        break;

                    case userrole.siteengineer:
                        consBase = consBase.Where(mc => mc.user_id == userId);
                        txBase = txBase.Where(t => t.to_user_id == userId);
                        break;

                    default:
                        return Forbid();
                }

                // 4) Build the two projections
                var consumptionQuery = consBase
                    .Join(_context.Sites,
                          mc => mc.site_id,
                          s => s.id,
                          (mc, s) => new CombinedTransactionDto
                          {
                              VoucherNo = "C" + mc.id,
                              SiteName = s.sitename,
                              Date = mc.date,
                              TransactionType = "Consumption",
                              CreatedOn = mc.createdon
                          });

                var transactionQuery = txBase
                    .Where(t => t.site_id.HasValue)
                    .Join(_context.Sites,
                          t => t.site_id!.Value,
                          s => s.id,
                          (t, s) => new CombinedTransactionDto
                          {
                              VoucherNo = t.transaction_type.Substring(0, 1).ToUpper() + t.id,
                              SiteName = s.sitename,
                              Date = t.transaction_date,
                              TransactionType = t.transaction_type,
                              CreatedOn = t.createdat
                          });

                // 5) Union, order, return
                var allTransactions = await consumptionQuery
                    .Union(transactionQuery)
                    .OrderByDescending(x => x.CreatedOn)
                    .ToListAsync();

                return Ok(new
                {
                    message = "Success",
                    data = allTransactions
                });
            }
            catch (Exception ex)
            {
                // Log ex here if you have a logger, e.g. _logger.LogError(ex, "…");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }




        public class CreateInterSiteTransactionDTO
        {
            [Required(ErrorMessage = "From site ID is required")]
            public int from_site_id { get; set; }

            [Required(ErrorMessage = "To site ID is required")]
            public int to_site_id { get; set; }

            public int? request_id { get; set; }
            public string? remark { get; set; }

            [Required(ErrorMessage = "Created by user is required")]
            public int createdby { get; set; }

            [Required(ErrorMessage = "At least one transaction item is required")]
            public List<CreateInterSiteTransactionItemDTO> items { get; set; } = new();
        }

        public class CreateInterSiteTransactionItemDTO
        {
            [Required(ErrorMessage = "Material ID is required")]
            public int material_id { get; set; }

            [Required(ErrorMessage = "Unit type ID is required")]
            public int unit_type_id { get; set; }

            [Required(ErrorMessage = "Quantity is required")]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int quantity { get; set; }
        }
            [HttpPost("CreateInterSiteTransaction")]
            public async Task<IActionResult> CreateInterSiteTransaction([FromBody] CreateInterSiteTransactionDTO transactionDto)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (var dbTransaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Validate sites
                        var fromSite = await _context.Sites.FindAsync(transactionDto.from_site_id);
                        var toSite = await _context.Sites.FindAsync(transactionDto.to_site_id);
                        if (fromSite == null || toSite == null)
                        {
                            return NotFound(new { message = "From or To site not found" });
                        }

                        if (transactionDto.from_site_id == transactionDto.to_site_id)
                        {
                            return BadRequest(new { message = "From and To sites must be different" });
                        }

                        // Validate createdby user
                        var createdByUser = await _context.Users.FindAsync(transactionDto.createdby);
                        if (createdByUser == null)
                        {
                            return NotFound(new { message = "Created by user not found" });
                        }

                        // Create transaction record
                        var newTransaction = new Transaction
                        {
                            transaction_type = "Issue To Site",
                            transaction_date = DateTime.Now,
                            from_site_id = transactionDto.from_site_id,
                            to_site_id = transactionDto.to_site_id,
                            createdby = transactionDto.createdby,
                            createdat = DateTime.Now,
                            updatedby = transactionDto.createdby,
                            updatedat = DateTime.Now,
                            status = "completed",
                            remark = transactionDto.remark,
                            request_id = transactionDto.request_id,
                            site_id = transactionDto.to_site_id // Assuming site_id refers to the originating site
                        };

                        _context.Transactions.Add(newTransaction);
                        await _context.SaveChangesAsync();

                        foreach (var itemDto in transactionDto.items)
                        {
                            // Validate material and unit
                            var material = await _context.Materials.FindAsync(itemDto.material_id);
                            var unit = await _context.Units.FindAsync(itemDto.unit_type_id);
                            if (material == null || unit == null)
                            {
                                throw new Exception("Material or Unit not found");
                            }

                            // Deduct from from_site stock
                            var fromStock = await _context.Stocks
                                .FromSqlInterpolated($@"
                            SELECT * FROM Stocks WITH (UPDLOCK)
                            WHERE site_id = {transactionDto.from_site_id}
                            AND material_id = {itemDto.material_id}
                            AND unit_type_id = {itemDto.unit_type_id}
                            AND StockOwnerType = 'site'")
                                .FirstOrDefaultAsync();

                            if (fromStock == null || fromStock.quantity < itemDto.quantity)
                            {
                                throw new Exception($"Insufficient stock in from site for material {material.id}");
                            }
                            fromStock.quantity -= itemDto.quantity;
                            fromStock.UpdatedAt = DateTime.Now;
                            fromStock.last_transaction_id = newTransaction.id;

                            // Add to to_site stock
                            var toStock = await _context.Stocks
                                .FromSqlInterpolated($@"
                            SELECT * FROM Stocks WITH (UPDLOCK)
                            WHERE site_id = {transactionDto.to_site_id}
                            AND material_id = {itemDto.material_id}
                            AND unit_type_id = {itemDto.unit_type_id}
                            AND StockOwnerType = 'site'")
                                .FirstOrDefaultAsync();

                            if (toStock == null)
                            {
                                toStock = new Stock
                                {
                                    site_id = transactionDto.to_site_id,
                                    material_id = itemDto.material_id,
                                    unit_type_id = itemDto.unit_type_id,
                                    StockOwnerType = "site",
                                    quantity = itemDto.quantity,
                                    UpdatedAt = DateTime.Now,
                                    last_transaction_id = newTransaction.id
                                };
                                _context.Stocks.Add(toStock);
                            }
                            else
                            {
                                toStock.quantity += itemDto.quantity;
                                toStock.UpdatedAt = DateTime.Now;
                                toStock.last_transaction_id = newTransaction.id;
                            }

                            // Create transaction item
                            var transactionItem = new Transaction_Items
                            {
                                transaction_id = newTransaction.id,
                                material_id = itemDto.material_id,
                                unit_type_id = itemDto.unit_type_id,
                                quantity = itemDto.quantity
                                // unit_price, gst, etc., can be null for inter-site
                            };
                            _context.Transaction_Items.Add(transactionItem);
                    }

                    await _context.SaveChangesAsync();
                    if (transactionDto.request_id != null)
                    {
                        var request = await _context.Material_Requests.FindAsync(transactionDto.request_id);

                        if (request == null)
                        {
                            throw new Exception("Request not found");
                        }
                        var opUser = await _context.Users.FindAsync(request.requested_by);
                        if (opUser == null)
                        {
                            throw new Exception("User not found");
                        }
                        // Check user's role and update status accordingly
                        if (opUser.role == userrole.sitemanager)
                        {
                            request.status = "Fulfilled";
                        }
                        if (opUser.role == userrole.siteengineer)
                        {
                            request.status = "Fulfilled To Site";
                        }
                        
                        
                        if (opUser.role == userrole.admin)
                        {
                            throw new Exception("User not ok");
                        }

                        request.approved_by = transactionDto.createdby;
                        request.approval_date = DateTime.Now;

                        await _context.SaveChangesAsync();
                    }

                    await dbTransaction.CommitAsync();

                        return Ok(new { message = "Transaction created successfully", transactionId = newTransaction.id });
                    }
                    catch (Exception ex)
                    {
                        await dbTransaction.RollbackAsync();
                        return StatusCode(500, new { message = $"Error creating transaction: {ex.Message}" });
                    }
                }
        }
    }
}