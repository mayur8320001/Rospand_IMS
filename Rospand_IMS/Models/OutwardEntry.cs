using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class OutwardEntry
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OutwardNumber { get; set; }

        [Required]
        public int SalesOrderId { get; set; }
        public SalesOrder SalesOrder { get; set; }

        [Required]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        [Required]
        public DateTime OutwardDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string CreatedBy { get; set; }

        public OutwardEntryStatus Status { get; set; } = OutwardEntryStatus.Processed;

        public ICollection<OutwardEntryItem> Items { get; set; } = new List<OutwardEntryItem>();
    }

    public class OutwardEntryItem
    {
        public int Id { get; set; }

        [Required]
        public int OutwardEntryId { get; set; }
        public OutwardEntry OutwardEntry { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }

        [StringLength(200)]
        public string Notes { get; set; }
    }
    public enum OutwardEntryStatus
    {
        Draft,
        Processed,
        Shipped,
        Delivered,
        Cancelled
    }
}
