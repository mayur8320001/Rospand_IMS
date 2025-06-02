using Microsoft.AspNetCore.Mvc.Rendering;

namespace Rospand_IMS.Models
{
    public class OutwardEntryCreateViewModel
    {
        public int SalesOrderId { get; set; }
        public string SONumber { get; set; }
        public string CustomerDisplayName { get; set; }
        public string OutwardNumber { get; set; }
        public DateTime OutwardDate { get; set; } = DateTime.UtcNow;
        public int WarehouseId { get; set; }
        public string Notes { get; set; }
        public List<OutwardEntryItemViewModel> Items { get; set; } = new List<OutwardEntryItemViewModel>();
        public SelectList Warehouses { get; set; }
    }

    public class OutwardEntryItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }
        public int QuantityOrdered { get; set; }
        public int QuantityAvailable { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
    }
}
