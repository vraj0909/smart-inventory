using System;
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Models
{
    public class StockTransaction
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [Required(ErrorMessage = "Transaction type is required.")]
        [Display(Name = "Transaction Type")]
        public TransactionType TransactionType { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 100000, ErrorMessage = "Quantity must be between 1 and 100,000.")]
        [Display(Name = "Quantity Change")]
        public int Quantity { get; set; }

        [Display(Name = "Previous Stock")]
        public int PreviousStock { get; set; }

        [Display(Name = "New Stock")]
        public int NewStock { get; set; }

        [Required(ErrorMessage = "Reason or note is required.")]
        [StringLength(250, ErrorMessage = "Reason cannot exceed 250 characters.")]
        [Display(Name = "Reason / Notes")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(60)]
        [Display(Name = "Performed By")]
        public string PerformedBy { get; set; } = "Store Admin";

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
