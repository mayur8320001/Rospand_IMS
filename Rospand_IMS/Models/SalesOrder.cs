using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rospand_IMS.Models
{
    public class SalesOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SONumber { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        [Required]
        public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;

        [Required]
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
    }

    public class SalesOrderItem
    {
        public int Id { get; set; }

        [Required]
        public int SalesOrderId { get; set; }
        public SalesOrder SalesOrder { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        [Range(0, 100)]
        public decimal TaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [StringLength(200)]
        public string Notes { get; set; }
    }

    public enum SalesOrderStatus
    {
        Draft,
        Confirmed,
        Picking,
        Packed,
        Shipped,
        Delivered,
        Cancelled,
        OnHold, // When waiting for stock
        PartiallyShipped

    }
}