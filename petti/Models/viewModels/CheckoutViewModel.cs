using System.ComponentModel.DataAnnotations;

namespace petti.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public CartDrawerViewModel Cart { get; set; } = new();

        [Required(ErrorMessage = "Recipient name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile phone number is required.")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amman area is required.")]
        [Display(Name = "Area / Neighborhood")]
        public string Area { get; set; } = string.Empty;

        [Required(ErrorMessage = "Detailed street address is required.")]
        [Display(Name = "Street & Building Details")]
        public string StreetAddress { get; set; } = string.Empty;

        [Display(Name = "Delivery Notes")]
        public string? DeliveryNotes { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "CashOnDelivery"; // "CashOnDelivery", "CardOnDelivery"
    }
}