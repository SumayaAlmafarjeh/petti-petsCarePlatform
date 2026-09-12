using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petti.Data;
using petti.Models;

namespace petti.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Orders
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.CustomerId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images) 
                .ToListAsync();

            return View(orders);
        }

        // GET: /Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images) 
                .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == user.Id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}