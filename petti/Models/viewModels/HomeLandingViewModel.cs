using petti.Models;

namespace petti.Models.ViewModels
{
    public class HomeLandingViewModel
    {
        public List<HomeServiceItemViewModel> Services { get; set; } = new();
        public List<HomeProductItemViewModel> BestSellingProducts { get; set; } = new();
        public List<Testimonial> Testimonials { get; set; } = new();
    }

    public class HomeServiceItemViewModel
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
    }

    public class HomeProductItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double AverageRating { get; set; } = 4.9;
        public int ReviewsCount { get; set; } = 24;
    }
}