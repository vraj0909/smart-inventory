using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Common;
using SmartInventory.Data;
using SmartInventory.Models;
using SmartInventory.ViewModels;

namespace SmartInventory.Controllers
{
    public class RestockController : Controller
    {
        private readonly InventoryDbContext _context;

        public RestockController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: Restock (View Session Restock Cart / Purchase Order Draft)
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<RestockItemSessionModel>>(SessionKeys.RestockCart) 
                ?? new List<RestockItemSessionModel>();

            var viewModel = new RestockOrderViewModel
            {
                Items = cart,
                TotalItemsCount = cart.Sum(i => i.SuggestedRestockQty),
                GrandTotalCost = cart.Sum(i => i.TotalItemCost),
                OrderReferenceNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{cart.Count}"
            };

            return View(viewModel);
        }

        // POST: Restock/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int? customQuantity, string? returnUrl)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            var cart = HttpContext.Session.GetObject<List<RestockItemSessionModel>>(SessionKeys.RestockCart) 
                ?? new List<RestockItemSessionModel>();

            var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);
            int qtyToAdd = customQuantity ?? Math.Max(product.ReorderLevel * 2 - product.QuantityInStock, 10);

            if (existingItem != null)
            {
                existingItem.SuggestedRestockQty += qtyToAdd;
            }
            else
            {
                cart.Add(new RestockItemSessionModel
                {
                    ProductId = product.Id,
                    SKU = product.SKU,
                    ProductName = product.Name,
                    CategoryName = product.Category?.Name ?? "General",
                    SupplierName = product.Supplier?.Name ?? "Unassigned",
                    SupplierId = product.SupplierId,
                    CurrentStock = product.QuantityInStock,
                    ReorderLevel = product.ReorderLevel,
                    SuggestedRestockQty = qtyToAdd,
                    UnitCostPrice = product.PurchasePrice,
                    Unit = product.Unit
                });
            }

            HttpContext.Session.SetObject(SessionKeys.RestockCart, cart);
            HttpContext.Session.RecordUserAction($"Added '{product.Name}' ({qtyToAdd} {product.Unit}) to Restock Draft PO");

            TempData["SuccessMessage"] = $"Added '{product.Name}' ({qtyToAdd} {product.Unit}) to your restock order plan.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Restock/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObject<List<RestockItemSessionModel>>(SessionKeys.RestockCart) 
                ?? new List<RestockItemSessionModel>();

            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.SuggestedRestockQty = quantity;
                }
                HttpContext.Session.SetObject(SessionKeys.RestockCart, cart);
                HttpContext.Session.RecordUserAction($"Adjusted restock quantity for '{item.ProductName}' to {quantity}");
                TempData["InfoMessage"] = $"Updated restock quantity for '{item.ProductName}'.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Restock/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            var cart = HttpContext.Session.GetObject<List<RestockItemSessionModel>>(SessionKeys.RestockCart) 
                ?? new List<RestockItemSessionModel>();

            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObject(SessionKeys.RestockCart, cart);
                HttpContext.Session.RecordUserAction($"Removed '{item.ProductName}' from Restock Draft");
                TempData["InfoMessage"] = $"Removed '{item.ProductName}' from restock draft.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Restock/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(SessionKeys.RestockCart);
            HttpContext.Session.RecordUserAction("Cleared Restock Draft basket");
            TempData["InfoMessage"] = "Restock draft order was cleared.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Restock/FinalizeOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeOrder(string? orderNotes)
        {
            var cart = HttpContext.Session.GetObject<List<RestockItemSessionModel>>(SessionKeys.RestockCart) 
                ?? new List<RestockItemSessionModel>();

            if (!cart.Any())
            {
                TempData["WarningMessage"] = "Your restock order draft is currently empty.";
                return RedirectToAction(nameof(Index));
            }

            string orderRef = $"PO-{DateTime.UtcNow:yyyyMMdd-HHmm}";
            int totalProcessedUnits = 0;

            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    int prev = product.QuantityInStock;
                    product.QuantityInStock += item.SuggestedRestockQty;
                    product.UpdatedAt = DateTime.UtcNow;

                    var transaction = new StockTransaction
                    {
                        ProductId = product.Id,
                        TransactionType = TransactionType.StockIn,
                        Quantity = item.SuggestedRestockQty,
                        PreviousStock = prev,
                        NewStock = product.QuantityInStock,
                        Reason = $"Restock Order Fulfillment #{orderRef}: {orderNotes ?? "Automated Supplier Replenishment"}",
                        PerformedBy = "Store Manager",
                        Timestamp = DateTime.UtcNow
                    };
                    _context.StockTransactions.Add(transaction);
                    totalProcessedUnits += item.SuggestedRestockQty;
                }
            }

            await _context.SaveChangesAsync();

            // Clear session cart
            HttpContext.Session.Remove(SessionKeys.RestockCart);
            HttpContext.Session.RecordUserAction($"Fulfilled Restock Order #{orderRef} ({totalProcessedUnits} units)");

            TempData["SuccessMessage"] = $"Purchase Order #{orderRef} confirmed! Replenished {totalProcessedUnits} units across {cart.Count} products. Stock levels updated.";
            return RedirectToAction("Index", "Stock");
        }
    }
}
