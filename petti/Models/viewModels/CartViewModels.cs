namespace petti.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = "Essentials";
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal Total => Price * Quantity;
    }

    public class CartDrawerViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal DiscountRate { get; set; } = 0.0m;

        public decimal Subtotal => Items.Sum(i => i.Total);
        public decimal DeliveryFee => Items.Any() ? 2.00m : 0.00m;
        public decimal Tax => Math.Round(Subtotal * 0.04m, 2);
        public decimal DiscountAmount => Math.Round(Subtotal * DiscountRate, 2);
        public decimal GrandTotal => Math.Max(0, Subtotal + DeliveryFee + Tax - DiscountAmount);
        public int TotalItemsCount => Items.Sum(i => i.Quantity);
    }
}