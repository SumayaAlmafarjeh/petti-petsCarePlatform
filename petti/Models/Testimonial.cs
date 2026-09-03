using System.ComponentModel.DataAnnotations;

namespace petti.Models
{
    public class Testimonial
    {
        public int TestimonialId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Pet Breed / Title")]
        public string? PetInfo { get; set; } 

        [Required]
        [MaxLength(500)]
        [Display(Name = "Testimonial Content")]
        public string Feedback { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        public string? ClientImageUrl { get; set; } = "/images/default-avatar.png";

        
        public bool IsFeatured { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public string? CustomerId { get; set; }
        public ApplicationUser? Customer { get; set; }
    }
}