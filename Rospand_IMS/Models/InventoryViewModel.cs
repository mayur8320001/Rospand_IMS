namespace Rospand_IMS.Models
{
    public class InventoryViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }
        public string CategoryName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityAvailable { get; set; }
        public DateTime LastUpdated { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal InventoryValue { get; set; }
        public decimal PotentialSalesValue { get; set; }
    }
}
