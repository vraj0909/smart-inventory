using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Common;
using SmartInventory.Data;
using SmartInventory.Models;
using SmartInventory.ViewModels;

namespace SmartInventory.Controllers
{
    public class ProductsController : Controller
    {
        private readonly InventoryDbContext _context;

        public ProductsController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(
            string? searchTerm,
            int? categoryId,
            int? supplierId,
            StockStatusFilter statusFilter = StockStatusFilter.All,
            string sortOrder = "name_asc",
            int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            int totalAllCount = await _context.Products.CountAsync();
            int lowStockCount = await _context.Products.CountAsync(p => p.QuantityInStock > 0 && p.QuantityInStock <= p.ReorderLevel);
            int outOfStockCount = await _context.Products.CountAsync(p => p.QuantityInStock <= 0);

            // Filtering
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) || p.SKU.ToLower().Contains(term));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (supplierId.HasValue && supplierId.Value > 0)
            {
                query = query.Where(p => p.SupplierId == supplierId.Value);
            }

            switch (statusFilter)
            {
                case StockStatusFilter.InStock:
                    query = query.Where(p => p.QuantityInStock > p.ReorderLevel);
                    break;
                case StockStatusFilter.LowStock:
                    query = query.Where(p => p.QuantityInStock > 0 && p.QuantityInStock <= p.ReorderLevel);
                    break;
                case StockStatusFilter.OutOfStock:
                    query = query.Where(p => p.QuantityInStock <= 0);
                    break;
            }

            // Sorting
            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(p => p.Name),
                "sku_asc" => query.OrderBy(p => p.SKU),
                "price_asc" => query.OrderBy(p => p.SellingPrice),
                "price_desc" => query.OrderByDescending(p => p.SellingPrice),
                "stock_asc" => query.OrderBy(p => p.QuantityInStock),
                "stock_desc" => query.OrderByDescending(p => p.QuantityInStock),
                _ => query.OrderBy(p => p.Name),
            };

            int totalMatching = await query.CountAsync();
            int pageSize = 8;
            int pageNumber = Math.Max(1, page);

            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            var suppliers = await _context.Suppliers.OrderBy(s => s.Name).ToListAsync();

            var viewModel = new ProductListViewModel
            {
                Products = products,
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                SupplierId = supplierId,
                StatusFilter = statusFilter,
                SortOrder = sortOrder,
                CategoryList = new SelectList(categories, "Id", "Name", categoryId),
                SupplierList = new SelectList(suppliers, "Id", "Name", supplierId),
                TotalMatchingCount = totalMatching,
                TotalAllCount = totalAllCount,
                LowStockCount = lowStockCount,
                OutOfStockCount = outOfStockCount,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.StockTransactions.OrderByDescending(t => t.Timestamp).Take(10))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            var model = new Product
            {
                QuantityInStock = 10,
                ReorderLevel = 10,
                Unit = "pcs",
                IsActive = true
            };
            return View(model);
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SKU,Name,Description,CategoryId,SupplierId,PurchasePrice,SellingPrice,QuantityInStock,ReorderLevel,Unit,IsActive")] Product product)
        {
            // Business rule validation: SellingPrice should not be less than PurchasePrice
            if (product.SellingPrice < product.PurchasePrice)
            {
                ModelState.AddModelError("SellingPrice", "Selling price should generally be equal to or higher than the cost purchase price.");
            }

            // Check SKU uniqueness
            if (await _context.Products.AnyAsync(p => p.SKU == product.SKU))
            {
                ModelState.AddModelError("SKU", "A product with this SKU already exists. Please choose a unique SKU.");
            }

            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.UtcNow;
                _context.Add(product);
                await _context.SaveChangesAsync();

                // If initial quantity > 0, log an initial stock transaction
                if (product.QuantityInStock > 0)
                {
                    var transaction = new StockTransaction
                    {
                        ProductId = product.Id,
                        TransactionType = TransactionType.StockIn,
                        Quantity = product.QuantityInStock,
                        PreviousStock = 0,
                        NewStock = product.QuantityInStock,
                        Reason = "Initial inventory setup upon product creation",
                        PerformedBy = "Store Manager",
                        Timestamp = DateTime.UtcNow
                    };
                    _context.StockTransactions.Add(transaction);
                    await _context.SaveChangesAsync();
                }

                // State Management: Record action in Session and notify via TempData
                HttpContext.Session.RecordUserAction($"Added product: '{product.Name}' (SKU: {product.SKU}) with {product.QuantityInStock} {product.Unit}");
                TempData["SuccessMessage"] = $"Product '{product.Name}' created successfully!";

                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(product.CategoryId, product.SupplierId);
            TempData["ErrorMessage"] = "Please correct the form errors below.";
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync(product.CategoryId, product.SupplierId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SKU,Name,Description,CategoryId,SupplierId,PurchasePrice,SellingPrice,QuantityInStock,ReorderLevel,Unit,IsActive,CreatedAt")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            // Business rule validation
            if (product.SellingPrice < product.PurchasePrice)
            {
                ModelState.AddModelError("SellingPrice", "Selling price should generally be equal to or higher than the cost purchase price.");
            }

            // Check SKU uniqueness excluding this product
            if (await _context.Products.AnyAsync(p => p.SKU == product.SKU && p.Id != product.Id))
            {
                ModelState.AddModelError("SKU", "Another product already uses this SKU.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if stock quantity changed directly to log transaction
                    var originalProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (originalProduct != null && originalProduct.QuantityInStock != product.QuantityInStock)
                    {
                        var diff = product.QuantityInStock - originalProduct.QuantityInStock;
                        var transaction = new StockTransaction
                        {
                            ProductId = product.Id,
                            TransactionType = TransactionType.Adjustment,
                            Quantity = Math.Abs(diff),
                            PreviousStock = originalProduct.QuantityInStock,
                            NewStock = product.QuantityInStock,
                            Reason = "Direct stock count manual override via Product Edit form",
                            PerformedBy = "Store Manager",
                            Timestamp = DateTime.UtcNow
                        };
                        _context.StockTransactions.Add(transaction);
                    }

                    product.UpdatedAt = DateTime.UtcNow;
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    // State management
                    HttpContext.Session.RecordUserAction($"Updated product: '{product.Name}' (Stock: {product.QuantityInStock})");
                    TempData["SuccessMessage"] = $"Product '{product.Name}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(product.CategoryId, product.SupplierId);
            TempData["ErrorMessage"] = "Validation failed. Please review the highlighted fields.";
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.StockTransactions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                string productName = product.Name;
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                HttpContext.Session.RecordUserAction($"Deleted product '{productName}' from catalog");
                TempData["SuccessMessage"] = $"Product '{productName}' was permanently removed.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/ExportCsv
        public async Task<IActionResult> ExportCsv()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderBy(p => p.Name)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,SKU,Name,Category,Supplier,CostPrice,SellingPrice,QuantityInStock,ReorderLevel,Unit,Status,InventoryCostVal,InventoryRetailVal");

            foreach (var p in products)
            {
                sb.AppendLine($"{p.Id},\"{p.SKU}\",\"{p.Name}\",\"{p.Category?.Name}\",\"{p.Supplier?.Name}\",{p.PurchasePrice},{p.SellingPrice},{p.QuantityInStock},{p.ReorderLevel},\"{p.Unit}\",\"{p.StockStatus}\",{p.InventoryCostValue},{p.InventoryRetailValue}");
            }

            HttpContext.Session.RecordUserAction("Exported products catalog to CSV");
            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            return File(buffer, "text/csv", $"Inventory_Report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        private async Task PopulateDropdownsAsync(int? selectedCategoryId = null, int? selectedSupplierId = null)
        {
            ViewBag.CategoryId = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", selectedCategoryId);
            ViewBag.SupplierId = new SelectList(await _context.Suppliers.OrderBy(s => s.Name).ToListAsync(), "Id", "Name", selectedSupplierId);
        }

        private async Task<bool> ProductExists(int id)
        {
            return await _context.Products.AnyAsync(e => e.Id == id);
        }
    }
}
