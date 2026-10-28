using System.ComponentModel.DataAnnotations;
using SmartInventory.Models;

namespace SmartInventory.ViewModels
{
    public class StockAdjustmentViewModel
    {
        [Required]
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public string Unit { get; set; } = "pcs";

        [Required(ErrorMessage = "Please select a transaction type.")]
        [Display(Name = "Adjustment Type")]
        public TransactionType TransactionType { get; set; } = TransactionType.StockIn;

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 100000, ErrorMessage = "Quantity must be at least 1.")]
        [Display(Name = "Quantity to Add / Deduct")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Please state a reason for this stock adjustment.")]
        [StringLength(250, MinimumLength = 3, ErrorMessage = "Reason must be between 3 and 250 characters.")]
        [Display(Name = "Reason / Reference (e.g., PO #1029, Damaged Stock, Counter Sale)")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Auditor / Staff Name")]
        [StringLength(50)]
        public string PerformedBy { get; set; } = "Staff Member";
    }
}
