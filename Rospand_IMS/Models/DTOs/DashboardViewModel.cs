namespace Rospand_IMS.Models.DTOs
{
    public class DashboardViewModel
    {
        public string Period { get; set; }

        // Counts
        public int PurchaseOrderCount { get; set; }
        public int SalesOrderCount { get; set; }
        public int OutwardEntryCount { get; set; }
        public int TotalStock { get; set; }

        // Financials
        public decimal PurchaseTotal { get; set; }
        public decimal SalesTotal { get; set; }
        public decimal Profit => SalesTotal - PurchaseTotal;

        // Charts data
        public List<string> TrendLabels { get; set; }
        public List<decimal> PurchaseTrends { get; set; }
        public List<decimal> SalesTrends { get; set; }

        // Recent transactions
        public List<PurchaseOrder> RecentPurchases { get; set; }
        public List<SalesOrder> RecentSales { get; set; }
    }
}
