using System.ComponentModel.DataAnnotations;

namespace petti.Models.ViewModels
{
    public class BookingWizardViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Please select a pet type.")]
        public string PetType { get; set; } = "Dog";

        [Required(ErrorMessage = "Pet name is required.")]
        public string PetName { get; set; } = string.Empty;

        public string? Breed { get; set; }
        public int? Age { get; set; }
        public string? BehavioralNotes { get; set; }

        [Required(ErrorMessage = "Please choose a booking date.")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Please select a time slot.")]
        public string TimeSlot { get; set; } = "09:00 – 11:00 AM";

        public string City { get; set; } = "Amman";

        [Required(ErrorMessage = "Area in Amman is required.")]
        public string Area { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street address is required.")]
        public string StreetAddress { get; set; } = string.Empty;

        public string? BuildingDetails { get; set; }

        [Required(ErrorMessage = "Contact phone number is required.")]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;
    }
}