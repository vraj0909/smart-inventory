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
    public class HomeController : Controller
    {
        private readonly InventoryDbContext _context;
        private readonly DatabaseStatusService _dbStatus;

        public HomeController(InventoryDbContext context, DatabaseStatusService dbStatus)
        {
            _context = context;
            _dbStatus = dbStatus;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync();

            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            var recentTransactions = await _context.StockTransactions
                .Include(t => t.Product)
                .OrderByDescending(t => t.Timestamp)
                .Take(7)
                .ToListAsync();

            var lowStockList = products.Where(p => p.IsLowStock).OrderBy(p => p.QuantityInStock).ToList();
            var outOfStockList = products.Where(p => p.IsOutOfStock).ToList();

            var restockCart = HttpContext.Session.GetObject<List<RestockItemSessionModel>>(SessionKeys.RestockCart) 
                ?? new List<RestockItemSessionModel>();

            var viewModel = new DashboardViewModel
            {
                TotalProducts = products.Count,
                TotalCategories = categories.Count,
                TotalSuppliers = await _context.Suppliers.CountAsync(),
                TotalStockUnits = products.Sum(p => p.QuantityInStock),

                HealthyStockCount = products.Count(p => !p.IsLowStock && !p.IsOutOfStock),
                LowStockCount = lowStockList.Count,
                OutOfStockCount = outOfStockList.Count,

                TotalInventoryCost = products.Sum(p => p.InventoryCostValue),
                TotalInventoryRetail = products.Sum(p => p.InventoryRetailValue),

                LowStockProducts = lowStockList,
                OutOfStockProducts = outOfStockList,
                RecentTransactions = recentTransactions,

                CategorySummaries = categories.Select(c => new CategorySummaryItem
                {
                    CategoryName = c.Name,
                    Icon = c.Icon,
                    ProductCount = c.Products.Count,
                    TotalUnits = c.Products.Sum(p => p.QuantityInStock),
                    TotalValue = c.Products.Sum(p => p.InventoryRetailValue)
                }).ToList(),

                // Session indicators
                LastActionSummary = HttpContext.Session.GetString(SessionKeys.LastActionMessage),
                SessionAuditCount = HttpContext.Session.GetInt32(SessionKeys.ActionCount) ?? 0,
                SessionRestockDraftCount = restockCart.Count,
                DatabaseStatus = _dbStatus.ProviderName,
                IsFallbackDatabase = !_dbStatus.IsConnectedToMySql
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
