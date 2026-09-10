namespace petti.Models.ViewModels
{
    public class ServiceCardViewModel
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public string? TargetPetType { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string IconClass { get; set; } = "fa-paw";
        public string ImageUrl { get; set; } = string.Empty;
        public double AverageRating { get; set; } = 5.0;
        public int ReviewsCount { get; set; } = 0;
    }
}