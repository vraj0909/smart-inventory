using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 60 characters.")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? Description { get; set; }

        [Display(Name = "Bootstrap Icon Class")]
        [StringLength(50)]
        public string Icon { get; set; } = "bi-tag";

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
