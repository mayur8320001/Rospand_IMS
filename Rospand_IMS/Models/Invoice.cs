using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rospand_IMS.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int SalesOrderId { get; set; }
        public SalesOrder SalesOrder { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        [StringLength(500)]
        public string? Notes { get; set; }  // Made nullable

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; } = 0;

        [NotMapped]
        public decimal BalanceDue => TotalAmount - AmountPaid;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }  // Made nullable

        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class InvoiceItem
    {
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } = 0;  // Default to 0

        [Required]
        [Range(0, 100)]
        public decimal TaxRate { get; set; } = 0;  // Default to 0

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }  // Made nullable
    }

    public enum InvoiceStatus
    {
        Draft,
        Sent,
        Paid,
        PartiallyPaid,
        Overdue,
        Cancelled,
        Refunded
    }

    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        public PaymentMethod Method { get; set; }

        [StringLength(100)]
        public string? TransactionReference { get; set; }  // Made nullable

        [StringLength(500)]
        public string? Notes { get; set; }  // Made nullable

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public enum PaymentMethod
    {
        Cash,
        CreditCard,
        BankTransfer,
        Check,
        OnlinePayment,
        Other
    }
}