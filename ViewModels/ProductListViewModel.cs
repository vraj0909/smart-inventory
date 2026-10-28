using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartInventory.Models;

namespace SmartInventory.ViewModels
{
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();

        // Filters
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? SupplierId { get; set; }
        public StockStatusFilter StatusFilter { get; set; } = StockStatusFilter.All;
        public string SortOrder { get; set; } = "name_asc";

        // Dropdowns
        public SelectList? CategoryList { get; set; }
        public SelectList? SupplierList { get; set; }

        // Metrics for filter bar
        public int TotalMatchingCount { get; set; }
        public int TotalAllCount { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalMatchingCount / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
