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
    public class ServicesController : Controller
    {
        private const string PendingBookingSessionKey = "PendingBookingDraft";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServicesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Services
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
                    CategoryName = s.Category.Name,
                    IconClass = s.Category.IconClass ?? "fa-paw",
                    ImageUrl = s.Images.Select(img => img.ImageUrl).FirstOrDefault() ?? "/images/placeholder-service.jpg",
                    AverageRating = s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 5.0,
                    ReviewsCount = s.Reviews.Count()
                })
                .ToListAsync();

            return View(services);
        }

        // GET: /Services/Book?serviceId=1
        // متاحة للزائر وللمستخدم المسجل
        [HttpGet]
        public async Task<IActionResult> Book(int? serviceId)
        {
            if (serviceId == null) return RedirectToAction(nameof(Index));

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
                PetName = user?.PetName ?? string.Empty,
                PetType = user?.PetType ?? "Dog",
                City = string.IsNullOrWhiteSpace(user?.City) ? "Amman" : user.City,
                Area = user?.Area ?? string.Empty,
                StreetAddress = user?.StreetAddress ?? string.Empty,
                ContactPhone = user?.PhoneNumber ?? string.Empty
            };

            return View(model);
        }

        // POST: /Services/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookingWizardViewModel model)
        {
            var service = await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId && s.IsActive);

            if (service == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.ServiceName = service.Name;
                model.ServicePrice = service.Price;
                return View(model);
            }

            // إذا لم يكن مسجلاً للدخول: نحفظ تفاصيل الحجز ونرسله للوجن
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                HttpContext.Session.SetObjectAsJson(PendingBookingSessionKey, model);
                var returnUrl = Url.Action(nameof(ResumeBooking), "Services");
                return Redirect($"/Identity/Account/Login?ReturnUrl={Uri.EscapeDataString(returnUrl ?? "/Services")}");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            return await SaveBookingAndRedirect(model, service, user);
        }

        // GET: /Services/ResumeBooking
        // بعد تسجيل الدخول يعود المستخدم إلى هنا تلقائياً لإنهاء الحجز
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ResumeBooking()
        {
            var draft = HttpContext.Session.GetObjectFromJson<BookingWizardViewModel>(PendingBookingSessionKey);
            if (draft == null) return RedirectToAction(nameof(Index));

            var service = await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ServiceId == draft.ServiceId && s.IsActive);

            if (service == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await SaveBookingAndRedirect(draft, service, user);
            HttpContext.Session.Remove(PendingBookingSessionKey);
            return result;
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

        private async Task<IActionResult> SaveBookingAndRedirect(BookingWizardViewModel model, Service service, ApplicationUser user)
        {
            var fullAddress = string.IsNullOrWhiteSpace(model.BuildingDetails)
                ? model.StreetAddress
                : $"{model.StreetAddress}, {model.BuildingDetails}";

            var booking = new Booking
            {
                ServiceId = service.ServiceId,
                CustomerId = user.Id,
                AppointmentDate = model.AppointmentDate,
                TimeSlot = model.TimeSlot,
                Status = "Confirmed",
                TotalPrice = service.Price,
                PetType = model.PetType,
                PetName = model.PetName,
                City = string.IsNullOrWhiteSpace(model.City) ? "Amman" : model.City,
                Area = model.Area,
                StreetAddress = fullAddress,
                ContactPhone = model.ContactPhone,
                Notes = string.IsNullOrWhiteSpace(model.BehavioralNotes) ? null : model.BehavioralNotes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Confirmation), new { id = booking.BookingId });
        }
    }
}