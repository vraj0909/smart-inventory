using System;
using System.Linq;
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
    public class StockController : Controller
    {
        private readonly InventoryDbContext _context;

        public StockController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: Stock (Audit Trail / Transaction History)
        public async Task<IActionResult> Index(int? productId, TransactionType? type, int page = 1)
        {
            var query = _context.StockTransactions
                .Include(t => t.Product)
                    .ThenInclude(p => p!.Category)
                .AsQueryable();

            if (productId.HasValue && productId.Value > 0)
            {
                query = query.Where(t => t.ProductId == productId.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(t => t.TransactionType == type.Value);
            }

            int pageSize = 12;
            int totalItems = await query.CountAsync();
            var transactions = await query
                .OrderByDescending(t => t.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Products = new SelectList(await _context.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", productId);
            ViewBag.SelectedProductId = productId;
            ViewBag.SelectedType = type;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.TotalItems = totalItems;

            return View(transactions);
        }

        // GET: Stock/Adjust/5
        public async Task<IActionResult> Adjust(int? id)
        {
            Product? product = null;
            if (id.HasValue)
            {
                product = await _context.Products.FindAsync(id.Value);
            }

            if (product == null)
            {
                product = await _context.Products.FirstOrDefaultAsync();
                if (product == null)
                {
                    TempData["WarningMessage"] = "Please add at least one product before performing stock adjustments.";
                    return RedirectToAction("Index", "Products");
                }
            }

            ViewBag.ProductList = new SelectList(await _context.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", product.Id);

            var model = new StockAdjustmentViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                SKU = product.SKU,
                CurrentStock = product.QuantityInStock,
                ReorderLevel = product.ReorderLevel,
                Unit = product.Unit,
                TransactionType = TransactionType.StockIn,
                Quantity = 1,
                PerformedBy = "Store Admin"
            };

            return View(model);
        }

        // POST: Stock/Adjust
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(StockAdjustmentViewModel model)
        {
            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null)
            {
                return NotFound();
            }

            // Validation: Cannot Stock Out more than existing inventory
            if (model.TransactionType == TransactionType.StockOut && model.Quantity > product.QuantityInStock)
            {
                ModelState.AddModelError("Quantity", $"Cannot deduct {model.Quantity} {product.Unit}. Only {product.QuantityInStock} {product.Unit} currently available in stock.");
            }

            if (ModelState.IsValid)
            {
                int previousStock = product.QuantityInStock;
                int newStock = previousStock;

                switch (model.TransactionType)
                {
                    case TransactionType.StockIn:
                    case TransactionType.Return:
                        newStock = previousStock + model.Quantity;
                        break;
                    case TransactionType.StockOut:
                        newStock = previousStock - model.Quantity;
                        break;
                    case TransactionType.Adjustment:
                        // Adjustment sets direct variance or reconciliation
                        newStock = model.Quantity;
                        break;
                }

                product.QuantityInStock = Math.Max(0, newStock);
                product.UpdatedAt = DateTime.UtcNow;

                var transaction = new StockTransaction
                {
                    ProductId = product.Id,
                    TransactionType = model.TransactionType,
                    Quantity = model.Quantity,
                    PreviousStock = previousStock,
                    NewStock = product.QuantityInStock,
                    Reason = model.Reason,
                    PerformedBy = string.IsNullOrWhiteSpace(model.PerformedBy) ? "Store Staff" : model.PerformedBy,
                    Timestamp = DateTime.UtcNow
                };

                _context.StockTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                // State management: Session audit tracking & TempData
                string actionSummary = $"{model.TransactionType} of {model.Quantity} {product.Unit} for '{product.Name}' (New Stock: {product.QuantityInStock})";
                HttpContext.Session.RecordUserAction(actionSummary);

                TempData["SuccessMessage"] = $"Stock updated! {actionSummary}";
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdown
            ViewBag.ProductList = new SelectList(await _context.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", model.ProductId);
            model.ProductName = product.Name;
            model.SKU = product.SKU;
            model.CurrentStock = product.QuantityInStock;
            model.ReorderLevel = product.ReorderLevel;
            model.Unit = product.Unit;

            TempData["ErrorMessage"] = "Could not process stock adjustment. Please check input errors.";
            return View(model);
        }

        // GET: Stock/LowStock
        // Dedicated view to resolve the problem: "tracking low inventory levels"
        public async Task<IActionResult> LowStock()
        {
            var lowStockItems = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.QuantityInStock <= p.ReorderLevel)
                .OrderBy(p => p.QuantityInStock)
                .ToListAsync();

            return View(lowStockItems);
        }

        // Quick API to get product stock details for dynamic JS dropdown in Adjust view
        [HttpGet]
        public async Task<IActionResult> GetProductStockInfo(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return Json(new
            {
                product.Id,
                product.SKU,
                product.Name,
                product.QuantityInStock,
                product.ReorderLevel,
                product.Unit,
                product.PurchasePrice
            });
        }
    }
}
