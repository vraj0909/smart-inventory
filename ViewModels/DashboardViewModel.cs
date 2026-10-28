using System.Collections.Generic;
using SmartInventory.Models;

namespace SmartInventory.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalStockUnits { get; set; }

        public int HealthyStockCount { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }

        public decimal TotalInventoryCost { get; set; }
        public decimal TotalInventoryRetail { get; set; }
        public decimal PotentialProfit => TotalInventoryRetail - TotalInventoryCost;
        public decimal OverallMargin => TotalInventoryRetail > 0 ? ((TotalInventoryRetail - TotalInventoryCost) / TotalInventoryRetail) * 100 : 0;

        public List<Product> LowStockProducts { get; set; } = new List<Product>();
        public List<Product> OutOfStockProducts { get; set; } = new List<Product>();
        public List<StockTransaction> RecentTransactions { get; set; } = new List<StockTransaction>();
        public List<CategorySummaryItem> CategorySummaries { get; set; } = new List<CategorySummaryItem>();

        // Session state indicators for UI
        public string? LastActionSummary { get; set; }
        public int SessionAuditCount { get; set; }
        public int SessionRestockDraftCount { get; set; }
        public string DatabaseStatus { get; set; } = "MySQL";
        public bool IsFallbackDatabase { get; set; }
    }

    public class CategorySummaryItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-tag";
        public int ProductCount { get; set; }
        public int TotalUnits { get; set; }
        public decimal TotalValue { get; set; }
    }
}
