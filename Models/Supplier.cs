using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Supplier company name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Supplier name must be between 2 and 100 characters.")]
        [Display(Name = "Supplier Company")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person name is required.")]
        [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters.")]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;

        [Required(ErrorMessage = "Supplier email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(120)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(25)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string? Address { get; set; }

        [StringLength(80, ErrorMessage = "City cannot exceed 80 characters.")]
        public string? City { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
