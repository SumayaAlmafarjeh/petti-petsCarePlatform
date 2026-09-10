namespace petti.Models.ViewModels
{
    public class AdminCustomerItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Area { get; set; }
        public DateTime RegisteredDate { get; set; }
        public int OrdersCount { get; set; }
        public int BookingsCount { get; set; }
        public bool IsLockedOut { get; set; }
    }
}