using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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
                .Include(p => p.Reviews)
                .OrderByDescending(p => _context.OrderItems
                    .Where(oi => oi.ProductId == p.ProductId)
                    .Sum(oi => (int?)oi.Quantity) ?? 0)
                .ThenByDescending(p => p.CreatedAt)
                .Take(8)
                .Select(p => new HomeProductItemViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryName = p.Category != null ? p.Category.Name : "General",
                    ImageUrl = p.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? "/images/placeholder-product.png",
                    AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 5.0,
                    ReviewsCount = p.Reviews.Count
                })
                .ToListAsync();

            var testimonials = await _context.Testimonials
                .AsNoTracking()
                .Where(t => t.Status == "Approved")
                .OrderByDescending(t => t.CreatedAt)
                .Take(3)
                .ToListAsync();

            
            bool canSubmit = false;
            if (_signInManager.IsSignedIn(User))
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerId == userId);
                    var hasBookings = await _context.Bookings.AnyAsync(b => b.CustomerId == userId);
                    canSubmit = hasOrders || hasBookings;
                }
            }

            var vm = new HomeLandingViewModel
            {
                Services = services,
                BestSellingProducts = products,
                Testimonials = testimonials,
                CanSubmitTestimonial = canSubmit
            };

            return View(vm);
        }

        // POST: /Home/SubmitTestimonial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTestimonial(int rating, string feedback)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return Json(new { success = false, message = "Please sign in first." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            
            var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerId == user.Id);
            var hasBookings = await _context.Bookings.AnyAsync(b => b.CustomerId == user.Id);

            if (!hasOrders && !hasBookings)
            {
                return Json(new { success = false, message = "You can only share feedback after completing a booking or order." });
            }

            if (string.IsNullOrWhiteSpace(feedback))
            {
                return Json(new { success = false, message = "Please write your feedback before submitting." });
            }

            
            string petDetails = "Verified Pet Parent";
            if (!string.IsNullOrWhiteSpace(user.PetName))
            {
                petDetails = $"Owner of {user.PetName}" + (!string.IsNullOrWhiteSpace(user.PetType) ? $" ({user.PetType})" : "");
            }

            var testimonial = new Testimonial
            {
                ClientName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : (user.UserName ?? "Pet Parent"),
                PetInfo = petDetails,
                Rating = Math.Clamp(rating, 1, 5),
                Feedback = feedback.Trim(),
                Status = "Pending",
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                CustomerId = user.Id,
                ClientImageUrl = null
            };

            _context.Testimonials.Add(testimonial);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}