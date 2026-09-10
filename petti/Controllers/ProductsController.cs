using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petti.Data;
using petti.Models.ViewModels;

namespace petti.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
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
                    ReviewsCount = p.Reviews.Count()
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
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

            if (product == null) return NotFound();
            
            return View(product);
        }
    }
}