using System.ComponentModel.DataAnnotations;

namespace petti.Models
{
    public class ServiceImage
    {
        public int ServiceImageId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; } = false;

        // Foreign Key
        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;
    }
}