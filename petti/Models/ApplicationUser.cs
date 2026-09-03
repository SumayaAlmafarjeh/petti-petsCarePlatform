using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace petti.Models
{
    public class ApplicationUser : IdentityUser
    {
       
        [Required]
        [MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        [MaxLength(50)]
        public string? City { get; set; } = "Amman";

        [MaxLength(100)]
        public string? Area { get; set; } 

        [MaxLength(250)]
        public string? StreetAddress { get; set; } 

        [MaxLength(50)]
        public string? PetType { get; set; } 

        [MaxLength(50)]
        public string? PetName { get; set; }

        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // (One-to-Many Relationships)
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}