using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petti.Data;
using petti.Models;
using petti.Models.ViewModels;

namespace petti.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProductCardViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    TargetPetType = p.TargetPetType ?? "All",
                    CategoryName = p.Category.Name,
                    CategoryId = p.CategoryId,
                    ImageUrl = p.Images.Select(img => img.ImageUrl).FirstOrDefault() ?? "/images/placeholder-product.png",
                    AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 5.0,
                    ReviewsCount = p.Reviews.Count
                })
                .ToListAsync();

            ViewBag.Categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive && c.Type == "Product")
                .ToListAsync();

            return View(products);
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.Customer)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

            if (product == null) return NotFound();

            bool canReview = false;
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    canReview = await _context.Orders
                        .AsNoTracking()
                        .Where(o => o.CustomerId == user.Id)
                        .AnyAsync(o => o.OrderItems.Any(oi => oi.ProductId == product.ProductId));
                }
            }

            ViewBag.CanReview = canReview;
            return View(product);
        }

        // POST: /Products/SubmitReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int productId, int rating, string comment)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Json(new { success = false, message = "Please sign in to write a review." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "User not found." });

            var hasPurchased = await _context.Orders
                .AsNoTracking()
                .Where(o => o.CustomerId == user.Id)
                .AnyAsync(o => o.OrderItems.Any(oi => oi.ProductId == productId));

            if (!hasPurchased)
            {
                return Json(new { success = false, message = "Only verified buyers can review this item." });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                return Json(new { success = false, message = "Review comment cannot be empty." });
            }

            var review = new Review
            {
                ProductId = productId,
                CustomerId = user.Id,
                Rating = Math.Clamp(rating, 1, 5),
                Comment = comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var customerName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : (user.UserName ?? "Verified Buyer");
            var initial = customerName.Trim()[0].ToString().ToUpper();

            return Json(new
            {
                success = true,
                author = customerName,
                initial = initial,
                rating = review.Rating,
                comment = review.Comment,
                date = review.CreatedAt.ToString("MMM dd, yyyy")
            });
        }
    }
}