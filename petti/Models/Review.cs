using System.ComponentModel.DataAnnotations;

namespace petti.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please write a comment.")]
        [MaxLength(600)]
        [Display(Name = "Review Comment")]
        public string Comment { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public string CustomerId { get; set; } = string.Empty;
        public ApplicationUser Customer { get; set; } = null!;

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? ServiceId { get; set; }
        public Service? Service { get; set; }
    }
}