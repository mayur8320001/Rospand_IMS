namespace Rospand_IMS.Models
{
    public class ConfirmSalesOrderViewModel
    {
        public int SalesOrderId { get; set; }
        public string SONumber { get; set; }
        public string CustomerName { get; set; }
        public List<StockIssueItemViewModel> OutOfStockItems { get; set; }
        public List<StockIssueItemViewModel> InsufficientStockItems { get; set; }
    }

    public class StockIssueItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int Available { get; set; }
    }
}
