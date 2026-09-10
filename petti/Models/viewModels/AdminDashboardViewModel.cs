using petti.Models;

namespace petti.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        // KPIs
        public decimal TotalRevenue { get; set; }
        public int TotalOrdersCount { get; set; }
        public int PendingOrdersCount { get; set; }
        public int TodayBookingsCount { get; set; }
        public int TotalCustomersCount { get; set; }

        // Feeds
        public List<Booking> TodayBookings { get; set; } = new();
        public List<Order> RecentOrders { get; set; } = new();
    }
}