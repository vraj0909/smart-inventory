using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventory.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "SKU (Stock Keeping Unit) is required.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "SKU must be between 3 and 30 characters.")]
        [RegularExpression(@"^[A-Z0-9-]+$", ErrorMessage = "SKU must contain only uppercase letters, numbers, and hyphens (e.g., TECH-101).")]
        [Display(Name = "Stock Keeping Unit (SKU)")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 120 characters.")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Required(ErrorMessage = "Please select a supplier.")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }
        public virtual Supplier? Supplier { get; set; }

        [Required(ErrorMessage = "Purchase cost price is required.")]
        [Range(0.01, 1000000.00, ErrorMessage = "Purchase price must be between 0.01 and 1,000,000.")]
        [Column(TypeName = "decimal(12, 2)")]
        [Display(Name = "Cost Price ($)")]
        [DataType(DataType.Currency)]
        public decimal PurchasePrice { get; set; }

        [Required(ErrorMessage = "Selling price is required.")]
        [Range(0.01, 1000000.00, ErrorMessage = "Selling price must be between 0.01 and 1,000,000.")]
        [Column(TypeName = "decimal(12, 2)")]
        [Display(Name = "Selling Price ($)")]
        [DataType(DataType.Currency)]
        public decimal SellingPrice { get; set; }

        [Required(ErrorMessage = "Quantity in stock is required.")]
        [Range(0, 100000, ErrorMessage = "Quantity must be between 0 and 100,000.")]
        [Display(Name = "Current Stock Quantity")]
        public int QuantityInStock { get; set; }

        [Required(ErrorMessage = "Reorder threshold level is required.")]
        [Range(1, 10000, ErrorMessage = "Reorder level must be at least 1.")]
        [Display(Name = "Low Stock Alert Threshold")]
        public int ReorderLevel { get; set; } = 10;

        [Required(ErrorMessage = "Unit of measurement is required.")]
        [StringLength(20)]
        [Display(Name = "Unit")]
        public string Unit { get; set; } = "pcs";

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Updated")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation property for audit logs
        public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

        // Computed status properties
        [NotMapped]
        public bool IsOutOfStock => QuantityInStock <= 0;

        [NotMapped]
        public bool IsLowStock => QuantityInStock > 0 && QuantityInStock <= ReorderLevel;

        [NotMapped]
        public string StockStatus => IsOutOfStock ? "Out of Stock" : (IsLowStock ? "Low Stock" : "In Stock");

        [NotMapped]
        public string StockBadgeClass => IsOutOfStock ? "danger" : (IsLowStock ? "warning" : "success");

        [NotMapped]
        public decimal InventoryCostValue => QuantityInStock * PurchasePrice;

        [NotMapped]
        public decimal InventoryRetailValue => QuantityInStock * SellingPrice;

        [NotMapped]
        public decimal ProfitMargin => SellingPrice > 0 ? Math.Round(((SellingPrice - PurchasePrice) / SellingPrice) * 100, 1) : 0;
    }
}
