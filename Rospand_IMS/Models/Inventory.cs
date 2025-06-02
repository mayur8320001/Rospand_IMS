using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rospand_IMS.Models
{
    public class Inventory
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        [Required]
        public int QuantityOnHand { get; set; }

        [Required]
        public int QuantityReserved { get; set; }

        [NotMapped]
        public int QuantityAvailable => QuantityOnHand - QuantityReserved;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class InventoryTransaction
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        [Required]
        public InventoryTransactionType TransactionType { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public int? PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
        public int? SalesOrderId { get; set; } // New field
        public SalesOrder? SalesOrder { get; set; } // New field

        public int? GoodsReceiptId { get; set; }
        public GoodsReceipt? GoodsReceipt { get; set; }

        [StringLength(200)]
        public string? ReferenceNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public enum InventoryTransactionType
    {
        Purchase,
        Sale,
        Adjustment,
        TransferIn,
        TransferOut,
        Return,
        Damage,
        Expired
    }

    public class Warehouse
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
