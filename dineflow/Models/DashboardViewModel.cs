namespace dineflow.Models
{
    public class DashboardViewModel
    {
        public int TotalReservations { get; set; }
        public int TotalTransactions { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodaySales { get; set; }
        public decimal MonthlySales { get; set; }
        public decimal YearlySales { get; set; }


    }


}
