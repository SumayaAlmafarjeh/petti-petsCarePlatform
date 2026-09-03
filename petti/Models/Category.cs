using Microsoft.CodeAnalysis;
using System.ComponentModel.DataAnnotations;

namespace petti.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [MaxLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

    
        [Required]
        [Display(Name = "Category Type")]
        public string Type { get; set; } = "Product"; // "Product" or "Service"

        // (Font Awesome class name  "fa-paw", "fa-bath")
        [MaxLength(50)]
        public string? IconClass { get; set; } = "fa-paw";

        public bool IsActive { get; set; } = true;

        
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}