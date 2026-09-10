using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using petti.Models;

namespace petti.Areas.Customer.Controllers
{
   
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public class ProfileViewModel
        {
            [Required(ErrorMessage = "Full name is required.")]
            [Display(Name = "Full name")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mobile number is required.")]
            [Phone(ErrorMessage = "Please enter a valid mobile number.")]
            [Display(Name = "Mobile number")]
            public string PhoneNumber { get; set; } = string.Empty;

            [Display(Name = "Amman area")]
            public string? City { get; set; }

            [Display(Name = "Street & building")]
            public string? Address { get; set; }

            public string Initials { get; set; } = "P";
            public string MemberSince { get; set; } = "2026";

            // Pet Profile Details
            [Display(Name = "Pet Name")]
            public string? PetName { get; set; }

            public string PetType { get; set; } = "Dog";
            public string PetBreed { get; set; } = "Companion";

            // Security / Password
            [DataType(DataType.Password)]
            public string? CurrentPassword { get; set; }

            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
            public string? NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match.")]
            public string? ConfirmPassword { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var initials = "PT";
            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                var parts = user.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                initials = parts.Length > 1
                    ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                    : $"{parts[0][0]}".ToUpper();
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                City = user.City ?? "Amman",
                PetName = string.IsNullOrWhiteSpace(user.PetName) ? "Add your pet" : user.PetName,
                PetType = string.IsNullOrWhiteSpace(user.PetType) ? "Dog" : user.PetType,
                PetBreed = "Companion",
                Initials = initials,
                MemberSince = user.CreatedAt.ToString("MMM yyyy")
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please check the entered profile details.";
                return RedirectToAction(nameof(Index));
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.City = model.City;
            if (!string.IsNullOrWhiteSpace(model.PetName))
            {
                user.PetName = model.PetName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile details saved successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(model.CurrentPassword) || string.IsNullOrEmpty(model.NewPassword))
            {
                TempData["ErrorMessage"] = "Please provide your current password and a new password.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Password updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePetProfile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.PetName = model.PetName;
            user.PetType = model.PetType;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Pet profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}