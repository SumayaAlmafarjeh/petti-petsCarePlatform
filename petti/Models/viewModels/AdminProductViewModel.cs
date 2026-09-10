using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace petti.Models.ViewModels
{
    public class AdminProductFormViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product title is required.")]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        public string TargetPetType { get; set; } = "Dog";

        [Range(0.01, 10000, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, 50000, ErrorMessage = "Stock must be 0 or more.")]
        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        [Display(Name = "Upload Product Images")]
        public List<IFormFile>? ImageFiles { get; set; }

        // رقم الإندكس للصورة الأساسية المختارة من الصور الجديدة المرفوعة
        public int PrimaryImageIndex { get; set; } = 0;

        // في حال اختيار صورة موجودة مسبقاً لتكون الأساسية
        public int? PrimaryExistingImageId { get; set; }

        public List<ExistingImageItem> ExistingImages { get; set; } = new();

        public List<SelectListItem> Categories { get; set; } = new();
    }

    public class ExistingImageItem
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}