using System.ComponentModel.DataAnnotations;

namespace petti.Models
{
    public class Service
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Service title is required.")]
        [MaxLength(150)]
        [Display(Name = "Service Title")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 1000, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Display(Name = "Estimated Duration (Minutes)")]
        [Range(10, 480)]
        public int DurationMinutes { get; set; } 

        [MaxLength(50)]
        public string? TargetPetType { get; set; } 

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Navigation Properties
        public ICollection<ServiceImage> Images { get; set; } = new List<ServiceImage>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}