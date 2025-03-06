namespace dineflow.Models
{
    public class DashboardViewModel
    {
        public int TotalMenu { get; set; } // Total number of menu items
        public decimal TodaySales { get; set; } // Total sales for today
        public decimal TotalRevenue { get; set; } // Total revenue
        public int TotalTransactions { get; set; } // Total number of transactions
        public List<OrderSummaryItem> OrderSummary { get; set; } // Top 5 most selling dishes
        public List<string> RevenueChartLabels { get; set; } // Labels for the revenue chart (e.g., months)
        public List<decimal> RevenueChartData { get; set; } // Data for the revenue chart
    }

    public class OrderSummaryItem
    {
        public string Category { get; set; } // Dish name
        public decimal Percentage { get; set; } // Percentage of total sales
    }


}
