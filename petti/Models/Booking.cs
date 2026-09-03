using System.ComponentModel.DataAnnotations;

namespace petti.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Appointment date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        
        [Required(ErrorMessage = "Time slot is required")]
        [MaxLength(50)]
        [Display(Name = "Time Slot")]
        public string TimeSlot { get; set; } = string.Empty;

        // Pending, Confirmed, Completed, Cancelled
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Service Price")]
        public decimal TotalPrice { get; set; }

        [MaxLength(50)]
        public string? PetType { get; set; } // Dog / Cat

        [MaxLength(50)]
        public string? PetName { get; set; }

        // Home Service Address
        [Required(ErrorMessage = "City is required")]
        [MaxLength(50)]
        public string City { get; set; } = "Amman";

        [Required(ErrorMessage = "Area is required")]
        [MaxLength(100)]
        public string Area { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street address is required")]
        [MaxLength(250)]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact phone is required")]
        [Phone]
        [MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Special Instructions / Notes")]
        public string? Notes { get; set; }

        // Foreign Keys
        public string CustomerId { get; set; } = string.Empty;
        public ApplicationUser Customer { get; set; } = null!;

        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;
    }
}