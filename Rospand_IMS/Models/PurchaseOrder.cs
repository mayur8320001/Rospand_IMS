using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public enum PurchaseOrderStatus
    {
        Draft,
        Submitted,
        Approved,
        Ordered,
        PartiallyReceived,
        Received,
        Cancelled
    }

    public class PurchaseOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string PONumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ExpectedDeliveryDate { get; set; }

        [Required]
        public int VendorId { get; set; }
        public Vendor Vendor { get; set; }

        public PurchaseOrderStatus Status { get; set; }

        [Required]
        public int CurrencyId { get; set; } 
        public Currency Currency { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCharges { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public string Notes { get; set; }

        public List<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();

        // Audit fields
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public string? SubmittedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? OrderedDate { get; set; }
        public string? OrderedBy { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string? ReceivedBy { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancelledBy { get; set; }
        public string? CancellationReason { get; set; }
    }

    public class PurchaseOrderItem
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        public decimal TaxRate { get; set; }
       

        public string Notes { get; set; }

        public int ReceivedQuantity { get; set; } = 0;
    }

}