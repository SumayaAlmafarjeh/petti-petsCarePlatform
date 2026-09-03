using System.ComponentModel.DataAnnotations;

namespace petti.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }

        [Range(0.01, 10000)]
        public decimal UnitPrice { get; set; }

        // Foreign Keys
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}