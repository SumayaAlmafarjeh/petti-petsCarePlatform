using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petti.Data;
using petti.Models;
using petti.Models.ViewModels;

namespace petti.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServicesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Services
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Price)
                .Select(s => new ServiceCardViewModel
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes,
                    TargetPetType = s.TargetPetType ?? "All",
                    CategoryName = s.Category != null ? s.Category.Name : "Care",
                    IconClass = s.Category != null ? s.Category.IconClass : "fa-paw",
                    ImageUrl = s.Images.Select(img => img.ImageUrl).FirstOrDefault() ?? "/images/placeholder-service.jpg",
                    AverageRating = s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 5.0,
                    ReviewsCount = s.Reviews.Count()
                })
                .ToListAsync();

            return View(services);
        }

        // GET: /Services/Book?serviceId=1
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Book(int serviceId)
        {
            if (serviceId <= 0) return RedirectToAction(nameof(Index));

            var service = await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.IsActive);

            if (service == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            var model = new BookingWizardViewModel
            {
                ServiceId = service.ServiceId,
                ServiceName = service.Name,
                ServicePrice = service.Price,
                DurationMinutes = service.DurationMinutes,
                AppointmentDate = DateTime.Today.AddDays(1),
                TimeSlot = "09:00 – 11:00 AM",
                PetName = user?.PetName ?? string.Empty,
                PetType = string.IsNullOrWhiteSpace(user?.PetType) ? "Dog" : user.PetType,
                City = "Amman",
                Area = user?.Area ?? "Abdoun",
                StreetAddress = user?.StreetAddress ?? string.Empty,
                ContactPhone = user?.PhoneNumber ?? string.Empty
            };

            return View(model);
        }

        // POST: /Services/Book
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookingWizardViewModel model)
        {
            var service = await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId && s.IsActive);

            if (service == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            
            ModelState.Remove("ServiceName");
            ModelState.Remove("City");

            if (!ModelState.IsValid)
            {
                model.ServiceName = service.Name;
                model.ServicePrice = service.Price;
                model.DurationMinutes = service.DurationMinutes;
                return View(model);
            }

            var fullAddress = string.IsNullOrWhiteSpace(model.BuildingDetails)
                ? model.StreetAddress
                : $"{model.StreetAddress}, {model.BuildingDetails}";

            var booking = new Booking
            {
                ServiceId = service.ServiceId,
                CustomerId = user.Id,
                AppointmentDate = model.AppointmentDate == default ? DateTime.Today.AddDays(1) : model.AppointmentDate,
                TimeSlot = string.IsNullOrWhiteSpace(model.TimeSlot) ? "09:00 – 11:00 AM" : model.TimeSlot,
                Status = "Confirmed",
                TotalPrice = service.Price,
                PetType = string.IsNullOrWhiteSpace(model.PetType) ? "Dog" : model.PetType,
                PetName = string.IsNullOrWhiteSpace(model.PetName) ? "Pet" : model.PetName,
                City = "Amman",
                Area = string.IsNullOrWhiteSpace(model.Area) ? "Amman" : model.Area,
                StreetAddress = fullAddress,
                ContactPhone = string.IsNullOrWhiteSpace(model.ContactPhone) ? (user.PhoneNumber ?? "0790000000") : model.ContactPhone,
                Notes = model.BehavioralNotes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Confirmation), new { id = booking.BookingId });
        }

        // GET: /Services/Confirmation/5
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.CustomerId == user.Id);

            if (booking == null) return NotFound();

            return View(booking);
        }
    }
}