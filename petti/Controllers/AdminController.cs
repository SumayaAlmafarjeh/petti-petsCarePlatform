using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using petti.Data;
using petti.Models;
using petti.Models.ViewModels;
using System.IO;

namespace petti.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;

            var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount)
                               + await _context.Bookings.Where(b => b.Status == "Confirmed" || b.Status == "Completed").SumAsync(b => b.TotalPrice);

            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            var todayBookingsCount = await _context.Bookings.CountAsync(b => b.AppointmentDate.Date == today);
            var customersCount = await _userManager.Users.CountAsync();

            var todayVisits = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Service)
                .Include(b => b.Customer)
                .OrderBy(b => b.AppointmentDate)
                .Take(5)
                .ToListAsync();

            var recentOrders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrdersCount = totalOrders,
                PendingOrdersCount = pendingOrders,
                TodayBookingsCount = todayBookingsCount,
                TotalCustomersCount = customersCount,
                TodayBookings = todayVisits,
                RecentOrders = recentOrders
            };

            return View(vm);
        }

        // POST: /Admin/ConfirmBooking
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = "Confirmed";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Products
        [HttpGet]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Images)
                .OrderByDescending(p => p.ProductId)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Type == "Product")
                .ToListAsync();

            return View("Products/Index", products);
        }

        // GET: /Admin/ProductCreate
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProductCreate()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Type == "Product")
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();

            var vm = new AdminProductFormViewModel
            {
                Categories = categories,
                IsActive = true
            };

            return View("Products/ProductEdit", vm);
        }

        // GET: /Admin/ProductEdit/5
        [HttpGet]
        [Authorize(Roles = "Admin")]    
        public async Task<IActionResult> ProductEdit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Type == "Product")
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();

            var vm = new AdminProductFormViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                TargetPetType = product.TargetPetType ?? "Dog",
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                ExistingImages = product.Images.Select((img, idx) => new ExistingImageItem
                {
                    ImageId = img.ProductImageId,
                    ImageUrl = img.ImageUrl,
                    IsPrimary = (idx == 0)
                }).ToList(),
                Categories = categories
            };

            return View("Products/ProductEdit", vm);
        }

        // POST: /Admin/ProductSave
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProductSave(AdminProductFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.Type == "Product")
                    .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                    .ToListAsync();

                if (model.ProductId > 0)
                {
                    
                    var dbImgs = await _context.ProductImages
                        .Where(img => img.ProductId == model.ProductId)
                        .ToListAsync();

                    model.ExistingImages = dbImgs.Select((img, idx) => new ExistingImageItem
                    {
                        ImageId = img.ProductImageId,
                        ImageUrl = img.ImageUrl,
                        IsPrimary = (idx == 0)
                    }).ToList();
                }

                return View("Products/ProductEdit", model);
            }

            Product product;

            if (model.ProductId == 0)
            {
                product = new Product
                {
                    Name = model.Name,
                    Description = model.Description ?? string.Empty,
                    CategoryId = model.CategoryId,
                    TargetPetType = model.TargetPetType,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Products.Add(product);
            }
            else
            {
                product = await _context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.ProductId == model.ProductId);

                if (product == null) return NotFound();

                product.Name = model.Name;
                product.Description = model.Description ?? string.Empty;
                product.CategoryId = model.CategoryId;
                product.TargetPetType = model.TargetPetType;
                product.Price = model.Price;
                product.StockQuantity = model.StockQuantity;
                product.IsActive = model.IsActive;

                
                if (model.PrimaryExistingImageId.HasValue)
                {
                    var chosen = product.Images.FirstOrDefault(i => i.ProductImageId == model.PrimaryExistingImageId.Value);
                    if (chosen != null)
                    {
                        var allImgs = product.Images.ToList();
                        allImgs.Remove(chosen);
                        allImgs.Insert(0, chosen);

                        product.Images.Clear();
                        foreach (var img in allImgs)
                        {
                            product.Images.Add(img);
                        }
                    }
                }
            }

           
            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var newImagesList = new List<ProductImage>();

                for (int i = 0; i < model.ImageFiles.Count; i++)
                {
                    var file = model.ImageFiles[i];
                    if (file.Length > 0)
                    {
                        string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var pImg = new ProductImage
                        {
                            ImageUrl = $"/uploads/products/{uniqueFileName}"
                        };

                        if (i == model.PrimaryImageIndex && !model.PrimaryExistingImageId.HasValue)
                        {
                            newImagesList.Insert(0, pImg);
                        }
                        else
                        {
                            newImagesList.Add(pImg);
                        }
                    }
                }

                foreach (var img in newImagesList)
                {
                    product.Images.Add(img);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product details and gallery saved successfully.";
            return RedirectToAction(nameof(Products));
        }

        // POST: /Admin/DeleteProductImage 
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProductImage(int imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null) return Json(new { success = false });

            if (!string.IsNullOrEmpty(image.ImageUrl) && image.ImageUrl.StartsWith("/uploads/"))
            {
                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);
            }

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // POST: /Admin/ProductToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProductToggleStatus(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = !product.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Product status changed to {(product.IsActive ? "Active" : "Hidden")}.";
            }
            return RedirectToAction(nameof(Products));
        }

        // POST: /Admin/ProductDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProductDelete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.OrderItems)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            if (product.OrderItems.Any())
            {
                product.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = "Product is linked to previous orders, so it was set to Hidden instead of deleted.";
            }
            else
            {
                foreach (var img in product.Images)
                {
                    if (!string.IsNullOrEmpty(img.ImageUrl) && img.ImageUrl.StartsWith("/uploads/"))
                    {
                        var path = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    }
                }

                _context.ProductImages.RemoveRange(product.Images);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted permanently from database.";
            }

            return RedirectToAction(nameof(Products));
        }
        // GET: /Admin/Categories
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .Include(c => c.Services)
                .OrderBy(c => c.Type)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return View("Categories/Index", categories);
        }

        // POST: /Admin/CategorySave
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CategorySave(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                TempData["WarningMessage"] = "Category name is required.";
                return RedirectToAction(nameof(Categories));
            }

            
            string DetermineIcon(string name, string type)
            {
                var n = name.ToLower();
                if (n.Contains("food") || n.Contains("treat") || n.Contains("feed")) return "fa-bone";
                if (n.Contains("toy") || n.Contains("play")) return "fa-baseball";
                if (n.Contains("groom") || n.Contains("hair") || n.Contains("cut")) return "fa-scissors";
                if (n.Contains("bath") || n.Contains("wash") || n.Contains("shower")) return "fa-shower";
                if (n.Contains("nail") || n.Contains("paw")) return "fa-paw";
                if (n.Contains("health") || n.Contains("med") || n.Contains("care")) return "fa-kit-medical";
                return type == "Service" ? "fa-spa" : "fa-box";
            }

            if (category.CategoryId == 0)
            {
                category.IconClass = DetermineIcon(category.Name, category.Type);
                _context.Categories.Add(category);
                TempData["SuccessMessage"] = "New category added successfully.";
            }
            else
            {
                var existing = await _context.Categories.FindAsync(category.CategoryId);
                if (existing == null) return NotFound();

                existing.Name = category.Name.Trim();
                existing.Type = category.Type;
                existing.Description = category.Description;
                if (string.IsNullOrWhiteSpace(existing.IconClass) || existing.IconClass == "fa-paw")
                {
                    existing.IconClass = DetermineIcon(existing.Name, existing.Type);
                }

                TempData["SuccessMessage"] = "Category updated successfully.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Categories));
        }

        // POST: /Admin/CategoryDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound();

            
            if (category.Products.Any() || category.Services.Any())
            {
                TempData["WarningMessage"] = $"Cannot delete '{category.Name}' because it has active products or services linked to it.";
                return RedirectToAction(nameof(Categories));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Category deleted permanently.";
            return RedirectToAction(nameof(Categories));
        }
        // GET: /Admin/Orders
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View("Orders/Index", orders);
        }

        // POST: /Admin/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return Json(new { success = false, message = "Order not found." });

            order.Status = status;
            await _context.SaveChangesAsync();

            return Json(new { success = true, status = order.Status });
        }
        // GET: /Admin/Services
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services
                .AsNoTracking()
                .Include(s => s.Category)
                .Include(s => s.Bookings)
                .OrderBy(s => s.Price)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Type == "Service")
                .ToListAsync();

            return View("Services/Index", services);
        }

        // POST: /Admin/ServiceSave
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ServiceSave(Service service)
        {
            if (string.IsNullOrWhiteSpace(service.Name) || service.Price <= 0 || service.DurationMinutes <= 0)
            {
                TempData["WarningMessage"] = "Please provide a valid service name, price, and duration.";
                return RedirectToAction(nameof(Services));
            }

            if (service.ServiceId == 0)
            {
                // Create
                service.IsActive = true;
                _context.Services.Add(service);
                TempData["SuccessMessage"] = "New doorstep service added successfully.";
            }
            else
            {
                // Edit
                var existing = await _context.Services.FindAsync(service.ServiceId);
                if (existing == null) return NotFound();

                existing.Name = service.Name.Trim();
                existing.Description = service.Description ?? string.Empty;
                existing.Price = service.Price;
                existing.DurationMinutes = service.DurationMinutes;
                existing.CategoryId = service.CategoryId;
                existing.TargetPetType = service.TargetPetType ?? "All";

                TempData["SuccessMessage"] = "Service updated successfully.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Services));
        }

        // POST: /Admin/ServiceToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ServiceToggleStatus(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                service.IsActive = !service.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Service status changed to {(service.IsActive ? "Active" : "Hidden")}.";
            }
            return RedirectToAction(nameof(Services));
        }

        // POST: /Admin/ServiceDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ServiceDelete(int id)
        {
            var service = await _context.Services
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.ServiceId == id);

            if (service == null) return NotFound();

            
            if (service.Bookings.Any())
            {
                service.IsActive = false; // Soft Delete
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = "Service is linked to appointment history, so it was set to Hidden instead of deleted.";
            }
            else
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service deleted permanently.";
            }

            return RedirectToAction(nameof(Services));
        }
        // GET: /Admin/Bookings
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Service)
                .Include(b => b.Customer)
                .OrderByDescending(b => b.AppointmentDate)
                .ThenBy(b => b.TimeSlot)
                .ToListAsync();

            return View("Bookings/Index", bookings);
        }

        // POST: /Admin/UpdateBookingStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, string status)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return Json(new { success = false, message = "Booking not found." });

            booking.Status = status;
            await _context.SaveChangesAsync();

            return Json(new { success = true, status = booking.Status });
        }

        // POST: /Admin/RescheduleBooking
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RescheduleBooking(int bookingId, DateTime appointmentDate, string timeSlot)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                TempData["WarningMessage"] = "Booking not found.";
                return RedirectToAction(nameof(Bookings));
            }

            booking.AppointmentDate = appointmentDate;
            booking.TimeSlot = timeSlot;
            booking.Status = "Confirmed"; 
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Booking #BK-{booking.BookingId} rescheduled successfully.";
            return RedirectToAction(nameof(Bookings));
        }
        // GET: /Admin/Users
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .AsNoTracking()
                .Include(u => u.Orders)
                .Include(u => u.Bookings)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AdminCustomerItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName ?? "Pet Owner",
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber ?? "—",
                    Area = u.Area ?? "Amman",
                    RegisteredDate = u.CreatedAt,
                    OrdersCount = u.Orders.Count,
                    BookingsCount = u.Bookings.Count,
                    IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow
                })
                .ToListAsync();

            return View("Users/Index", users);
        }

        // POST: /Admin/ToggleUserStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Json(new { success = false, message = "User not found." });

            bool isCurrentlyLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

            if (isCurrentlyLocked)
            {
            
                user.LockoutEnd = null;
            }
            else
            { 
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            }

            var result = await _userManager.UpdateAsync(user);
            return Json(new { success = result.Succeeded, isLocked = !isCurrentlyLocked });
        }
        // GET: /Admin/Reviews
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.Customer)
                .Include(r => r.Product)
                .Include(r => r.Service)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.PendingCount = reviews.Count(r => r.Status == "Pending");
            ViewBag.ApprovedCount = reviews.Count(r => r.Status == "Approved");
            ViewBag.RejectedCount = reviews.Count(r => r.Status == "Rejected");

            return View("Reviews/Index", reviews);
        }

        // POST: /Admin/ModerateReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ModerateReview(int reviewId, string status)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return Json(new { success = false, message = "Review not found." });

            review.Status = status;
            await _context.SaveChangesAsync();

            var pending = await _context.Reviews.CountAsync(r => r.Status == "Pending");
            var approved = await _context.Reviews.CountAsync(r => r.Status == "Approved");
            var rejected = await _context.Reviews.CountAsync(r => r.Status == "Rejected");

            return Json(new { success = true, status = review.Status, pending, approved, rejected });
        }

        // POST: /Admin/DeleteReview/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Review removed permanently.";
            }
            return RedirectToAction(nameof(Reviews));
        }

        // GET: /Admin/Testimonials
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Testimonials()
        {
            var testimonials = await _context.Testimonials
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.PendingCount = testimonials.Count(t => t.Status == "Pending");
            ViewBag.ApprovedCount = testimonials.Count(t => t.Status == "Approved");
            ViewBag.RejectedCount = testimonials.Count(t => t.Status == "Rejected");

            return View("Testimonials/Index", testimonials);
        }

        // POST: /Admin/ModerateTestimonial
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ModerateTestimonial(int id, string status)
        {
            var item = await _context.Testimonials.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Testimonial not found." });

            item.Status = status;
            item.IsFeatured = (status == "Approved");
            await _context.SaveChangesAsync();

            var pending = await _context.Testimonials.CountAsync(t => t.Status == "Pending");
            var approved = await _context.Testimonials.CountAsync(t => t.Status == "Approved");
            var rejected = await _context.Testimonials.CountAsync(t => t.Status == "Rejected");

            return Json(new { success = true, status = item.Status, pending, approved, rejected });
        }

        // POST: /Admin/DeleteTestimonial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTestimonial(int id)
        {
            var item = await _context.Testimonials.FindAsync(id);
            if (item != null)
            {
                _context.Testimonials.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Testimonial deleted permanently.";
            }
            return RedirectToAction(nameof(Testimonials));
        }
    }
}