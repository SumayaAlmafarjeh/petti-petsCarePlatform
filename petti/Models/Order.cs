using System.ComponentModel.DataAnnotations;

namespace petti.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Payment Method")]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash on Delivery";

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
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DeliveryNotes { get; set; }

        // Foreign Key
        public string CustomerId { get; set; } = string.Empty;
        public ApplicationUser Customer { get; set; } = null!;

        // Navigation Property
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}