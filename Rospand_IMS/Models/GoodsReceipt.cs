using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
   
        public enum GoodsReceiptStatus
        {
            Pending,
            PartiallyReceived,
            Completed,
            Cancelled
        }

        public class GoodsReceipt
        {
            public int Id { get; set; }

            [Required]
            [StringLength(20)]
            public string GRNumber { get; set; } = GenerateGRNumber();

            [Required]
            public DateTime ReceiptDate { get; set; } = DateTime.Today;

            [Required]
            public int PurchaseOrderId { get; set; }
            public PurchaseOrder PurchaseOrder { get; set; }

            [Required]
            public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Pending;

            [StringLength(500)]
            public string? Notes { get; set; }

            public ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();

            public static string GenerateGRNumber()
            {
                return "GR-" + DateTime.Now.ToString("yyyyMMdd");
            }
        }

        public class GoodsReceiptItem
        {
            public int Id { get; set; }

            [Required]
            public int GoodsReceiptId { get; set; }
            public GoodsReceipt GoodsReceipt { get; set; }

            [Required]
            public int PurchaseOrderItemId { get; set; }
            public PurchaseOrderItem PurchaseOrderItem { get; set; }

            [Required]
            [Range(1, int.MaxValue)]
            public int QuantityReceived { get; set; }

            public decimal UnitPrice { get; set; }

            [StringLength(200)]
            public string? BatchNumber { get; set; }

            public DateTime? ExpiryDate { get; set; }

            [StringLength(500)]
            public string? Notes { get; set; }
        }
    }

