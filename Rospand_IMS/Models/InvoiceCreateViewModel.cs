using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class InvoiceCreateViewModel
    {
        public int SalesOrderId { get; set; }
        public string SONumber { get; set; }
        public string CustomerName { get; set; }
      

        [Required]
        public DateTime InvoiceDate { get; set; } = DateTime.Today;

        [Required]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);

        public string Notes { get; set; }

        public List<InvoiceItemViewModel> Items { get; set; } = new List<InvoiceItemViewModel>();
    }
    public class InvoiceIndexViewModel
    {
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public string SearchString { get; set; }
        public InvoiceStatus? StatusFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SortOrder { get; set; }
    }
    public class InvoiceItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TaxRate { get; set; }
        public string Description { get; set; }
    }

    public class InvoiceDetailsViewModel
    {
        public Invoice Invoice { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal BalanceDue { get; set; }
        public List<Payment> Payments { get; set; }
    }
    public class PaymentViewModel
    {
        public int InvoiceId { get; set; }

        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; }

        [Required(ErrorMessage = "Payment date is required")]
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Payment Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public PaymentMethod Method { get; set; }

        [Display(Name = "Reference/Transaction ID")]
        [StringLength(100, ErrorMessage = "Reference cannot exceed 100 characters")]
        public string TransactionReference { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        // For displaying the maximum allowed payment amount
        public decimal MaxAmount { get; set; }
    }
}
