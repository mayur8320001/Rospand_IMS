using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class GoodsReceiptCreateViewModel
    {
      
        public int PurchaseOrderId { get; set; }
        public string PONumber { get; set; }
        public string VendorName { get; set; }
        public string GRNumber { get; set; } = GoodsReceipt.GenerateGRNumber();

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReceiptDate { get; set; }

        [Required]
        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        public string? Notes { get; set; }

        public List<GoodsReceiptItemViewModel> Items { get; set; } = new List<GoodsReceiptItemViewModel>();

        public List<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
     
    }

    public class GoodsReceiptItemViewModel
    {
        public int PurchaseOrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int OrderedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
        public int QuantityReceived { get; internal set; }
    }
}