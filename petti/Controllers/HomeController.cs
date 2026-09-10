using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petti.Data;
using petti.Models;
using petti.Models.ViewModels;

namespace petti.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .AsNoTracking()
                .Where(s => s.IsActive)
                .Include(s => s.Category)
                .Select(s => new HomeServiceItemViewModel
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes,
                    CategoryName = s.Category != null ? s.Category.Name : "General",
                    IconClass = s.Category != null ? s.Category.IconClass : "fa-scissors"
                })
                .ToListAsync();

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Take(8)
                .Select(p => new HomeProductItemViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryName = p.Category != null ? p.Category.Name : "General",
                    ImageUrl = p.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? "/images/placeholder-product.png",
                    AverageRating = 4.9,
                    ReviewsCount = 18
                })
                .ToListAsync();

            var testimonials = await _context.Testimonials
                   .AsNoTracking()
                   .Where(t => t.Status == "Approved")
                   .OrderByDescending(t => t.CreatedAt)
                   .Take(3)
                   .ToListAsync();

            var vm = new HomeLandingViewModel
            {
                Services = services,
                BestSellingProducts = products,
                Testimonials = testimonials
            };

            return View(vm);
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}