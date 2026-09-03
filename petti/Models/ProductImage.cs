using Microsoft.CodeAnalysis;
using System.ComponentModel.DataAnnotations;

namespace petti.Models
{
    public class ProductImage
    {
        public int ProductImageId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        
        public bool IsMain { get; set; } = false;

      
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}