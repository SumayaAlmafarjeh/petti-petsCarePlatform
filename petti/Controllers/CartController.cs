using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petti.Data;
using petti.Extensions;
using petti.Models;
using petti.Models.ViewModels;

namespace petti.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "PettiCartSession";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private List<CartItemViewModel> GetCart()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>(CartSessionKey)
                   ?? new List<CartItemViewModel>();
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
        }

        // GET: /Cart
        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            var cart = GetCart();
            var model = new CartDrawerViewModel { Items = cart };
            return View(model);
        }

        // GET: /Cart/GetSummary
        [Authorize]
        [HttpGet]
        public IActionResult GetSummary()
        {
            var cart = GetCart();
            var model = new CartDrawerViewModel { Items = cart };
            return Json(new
            {
                itemsCount = model.TotalItemsCount,
                subtotal = model.Subtotal.ToString("0.00"),
                grandTotal = model.GrandTotal.ToString("0.00"),
                items = model.Items
            });
        }

        // POST: /Cart/AddToCart
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive);

            if (product == null) return NotFound(new { success = false, message = "Product unavailable" });

            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Price = product.Price,
                    ImageUrl = product.Images.Select(img => img.ImageUrl).FirstOrDefault() ?? "/images/placeholder-product.png",
                    Quantity = quantity
                });
            }

            SaveCart(cart);

            var model = new CartDrawerViewModel { Items = cart };
            return Json(new
            {
                success = true,
                itemsCount = model.TotalItemsCount,
                productName = product.Name,
                price = product.Price.ToString("0.00"),
                img = product.Images.Select(img => img.ImageUrl).FirstOrDefault() ?? "/images/placeholder-product.png"
            });
        }

        // POST: /Cart/UpdateQuantity
        [Authorize]
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                if (quantity <= 0) cart.Remove(item);
                else item.Quantity = quantity;
                SaveCart(cart);
            }

            var model = new CartDrawerViewModel { Items = cart };
            return Json(new
            {
                success = true,
                itemsCount = model.TotalItemsCount,
                subtotal = model.Subtotal.ToString("0.00"),
                grandTotal = model.GrandTotal.ToString("0.00")
            });
        }

        // POST: /Cart/RemoveItem
        [Authorize]
        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.ProductId == productId);
            SaveCart(cart);

            var model = new CartDrawerViewModel { Items = cart };
            return Json(new
            {
                success = true,
                itemsCount = model.TotalItemsCount,
                subtotal = model.Subtotal.ToString("0.00"),
                grandTotal = model.GrandTotal.ToString("0.00")
            });
        }

        // GET: /Cart/Checkout
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            var user = await _userManager.GetUserAsync(User);

            var model = new CheckoutViewModel
            {
                Cart = new CartDrawerViewModel { Items = cart },
                FullName = user?.FullName ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty,
                Area = user?.Area ?? string.Empty,
                StreetAddress = user?.StreetAddress ?? string.Empty
            };

            return View(model);
        }

        // POST: /Cart/Checkout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // إزالة التحقق من Cart داخل ModelState لأنها لا تُرسل من الفورم
            ModelState.Remove("Cart");

            if (!ModelState.IsValid)
            {
                model.Cart = new CartDrawerViewModel { Items = cart };
                return View(model);
            }

            var cartDrawer = new CartDrawerViewModel { Items = cart };

            // 1. إنشاء وحفظ الطلب
            var order = new Order
            {
                CustomerId = user.Id,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                PaymentMethod = model.PaymentMethod ?? "CashOnDelivery",
                TotalAmount = cartDrawer.GrandTotal,
                City = "Amman",
                Area = string.IsNullOrWhiteSpace(model.Area) ? "Amman" : model.Area,
                StreetAddress = string.IsNullOrWhiteSpace(model.StreetAddress) ? "Street Address" : model.StreetAddress,
                PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? (user.PhoneNumber ?? "0790000000") : model.PhoneNumber,
                DeliveryNotes = model.DeliveryNotes,
                OrderItems = cart.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                }).ToList()
            };

            _context.Orders.Add(order);

            // 2. تحديث مخزون المنتجات
            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
                }
            }

            // حفظ كل التغييرات وتوليد OrderId حقيقي
            await _context.SaveChangesAsync();

            // 3. تفريغ السلة من الـ Session
            HttpContext.Session.Remove(CartSessionKey);

            // 4. التوجيه لصفحة النجاح برقم الطلب الحقيقي
            return RedirectToAction(nameof(OrderSuccess), new { orderId = order.OrderId });
        }

        // GET: /Cart/OrderSuccess
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderSuccess(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == user.Id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}